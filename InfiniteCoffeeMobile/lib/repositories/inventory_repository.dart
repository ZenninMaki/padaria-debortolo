import 'dart:convert';
import 'dart:async';
import 'dart:io';
import 'dart:math';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http show ClientException;
import 'package:shared_preferences/shared_preferences.dart';

import '../models/product.dart';
import '../services/inventory_api.dart';

class InventoryRepository {
  InventoryRepository(this._api);

  static const _productsKey = 'cached_products';
  static const _pendingKey = 'pending_stock_exits';
  static const _maxAttempts = 20;
  static const _maxConcurrent = 5;
  final InventoryApi _api;
  final Future<SharedPreferences> _prefsFuture =
      SharedPreferences.getInstance();

  Future<InventorySnapshot> load({String search = ''}) async {
    try {
      final products = await _api.getStock(search: search);
      await _saveProducts(products);
      unawaited(_syncPending());
      return InventorySnapshot(products: products, isOffline: false);
    } catch (error) {
      final products = await _readProducts();
      final term = search.trim().toLowerCase();
      final filtered = term.isEmpty
          ? products
          : products
                .where(
                  (product) =>
                      product.name.toLowerCase().contains(term) ||
                      (product.barcode ?? '').contains(term),
                )
                .toList();
      final message = switch (error) {
        ApiException(:final message) => message,
        TimeoutException() => 'O servidor demorou para responder. Tente novamente em alguns segundos.',
        _ => 'Nao foi possivel conectar a API. Verifique a rede e se o servidor esta ligado.',
      };
      return InventorySnapshot(
        products: filtered,
        isOffline: true,
        errorMessage: message,
      );
    }
  }

  Future<InventorySnapshot> syncNow({String search = ''}) async {
    await _syncPending();
    return load(search: search);
  }

  Future<void> backup() => _api.backup();

  Future<ExitResult> registerExit({
    required Product product,
    required int quantity,
    required String reason,
  }) async {
    if (quantity < 1) {
      return const ExitResult(false, 'Informe uma quantidade valida.');
    }
    if (quantity > product.quantity) {
      return const ExitResult(false, 'Estoque insuficiente.');
    }
    final id = _newOpId();
    try {
      await _api.registerExit(
        productId: product.id,
        quantity: quantity,
        reason: reason,
        idempotencyKey: id,
      );
      await _changeCachedQuantity(product.id, -quantity);
      return const ExitResult(true, 'Saida registrada com sucesso.');
    } catch (error) {
      if (!_isRetryable(error)) {
        return ExitResult(false, _userMessage(error));
      }
      await _enqueue(
        tipo: 'saida',
        id: id,
        productId: product.id,
        quantity: quantity,
        reason: reason,
      );
      await _changeCachedQuantity(product.id, -quantity);
      return const ExitResult(
        true,
        'Saida salva offline e sera sincronizada quando houver internet.',
      );
    }
  }

  Future<ExitResult> registerEntry({
    required Product product,
    required int quantity,
    required String reason,
  }) async {
    if (quantity < 1) {
      return const ExitResult(false, 'Informe uma quantidade valida.');
    }
    final id = _newOpId();
    try {
      await _api.registerEntry(
        productId: product.id,
        quantity: quantity,
        reason: reason,
        idempotencyKey: id,
      );
      await _changeCachedQuantity(product.id, quantity);
      return const ExitResult(true, 'Entrada registrada com sucesso.');
    } catch (error) {
      if (!_isRetryable(error)) {
        return ExitResult(false, _userMessage(error));
      }
      await _enqueue(
        tipo: 'entrada',
        id: id,
        productId: product.id,
        quantity: quantity,
        reason: reason,
      );
      await _changeCachedQuantity(product.id, quantity);
      return const ExitResult(
        true,
        'Entrada salva offline e sera sincronizada depois.',
      );
    }
  }

  Future<ExitResult> createProduct({
    required String name,
    required String description,
    required String barcode,
    required String type,
    required double price,
    required int quantity,
  }) async {
    try {
      await _api.createProduct(
        name: name,
        description: description,
        barcode: barcode,
        type: type,
        price: price,
        quantity: quantity,
      );
      return const ExitResult(true, 'Produto cadastrado com sucesso.');
    } catch (error) {
      debugPrint('[CreateProduct] Falha ao cadastrar produto "$name": $error');
      return const ExitResult(
        false,
        'Cadastros de produtos exigem conexão com o servidor.',
      );
    }
  }

  Future<void> _syncPending() async {
    final preferences = await _prefsFuture;
    final pending = preferences.getStringList(_pendingKey) ?? [];
    if (pending.isEmpty) return;

    final remaining = <String>[];

    for (var i = 0; i < pending.length; i += _maxConcurrent) {
      final end =
          i + _maxConcurrent > pending.length ? pending.length : i + _maxConcurrent;
      final results = await Future.wait(
        pending.sublist(i, end).map(_attemptSync),
      );
      for (final r in results) {
        if (r != null) remaining.add(r);
      }
    }

    await preferences.setStringList(_pendingKey, remaining);
  }

  /// Retorna null se sincronizou ou descartou (fatal/limite); senão o JSON atualizado.
  Future<String?> _attemptSync(String item) async {
    Map<String, dynamic> data;
    try {
      data = jsonDecode(item) as Map<String, dynamic>;
    } catch (_) {
      debugPrint('[SyncPending] Item ilegivel descartado.');
      return null;
    }
    final tipo = '${data['tipo'] ?? 'saida'}';
    final isEntry = tipo == 'entrada';
    final id = '${data['id'] ?? _newOpId()}';
    final attempts = (data['attempts'] as num?)?.toInt() ?? 0;
    try {
      if (isEntry) {
        await _api.registerEntry(
          productId: (data['produtoId'] as num).toInt(),
          quantity: (data['quantidade'] as num).toInt(),
          reason: '${data['motivo'] ?? ''}',
          idempotencyKey: id,
        );
      } else {
        await _api.registerExit(
          productId: (data['produtoId'] as num).toInt(),
          quantity: (data['quantidade'] as num).toInt(),
          reason: '${data['motivo'] ?? ''}',
          idempotencyKey: id,
        );
      }
      return null;
    } catch (error) {
      if (!_isRetryable(error)) {
        debugPrint('[SyncPending] Erro fatal, descartado: $error');
        return null;
      }
      if (attempts + 1 >= _maxAttempts) {
        debugPrint('[SyncPending] Limite de tentativas, descartado: $id');
        return null;
      }
      debugPrint('[SyncPending] Retryavel ($tipo id=$id): $error');
      return jsonEncode({...data, 'id': id, 'tipo': tipo, 'attempts': attempts + 1});
    }
  }

  bool _isRetryable(Object error) {
    if (error is TimeoutException) return true;
    if (error is SocketException) return true;
    if (error is http.ClientException) return true;
    if (error is ApiException) {
      final s = error.statusCode;
      if (s == null) return true;
      if (s == 408 || s == 429) return true;
      return s >= 500;
    }
    return false;
  }

  String _userMessage(Object error) => switch (error) {
        ApiException(:final message) => message,
        TimeoutException() => 'O servidor demorou para responder.',
        _ => 'Operacao rejeitada pelo servidor.',
      };

  String _newOpId() =>
      '${DateTime.now().microsecondsSinceEpoch}-${Random.secure().nextInt(1 << 32)}';

  Future<void> _enqueue({
    required String tipo,
    required String id,
    required int productId,
    required int quantity,
    required String reason,
  }) async {
    final preferences = await _prefsFuture;
    final pending = preferences.getStringList(_pendingKey) ?? [];
    pending.add(
      jsonEncode({
        'v': 1,
        'id': id,
        'tipo': tipo,
        'produtoId': productId,
        'quantidade': quantity,
        'motivo': reason,
        'ts': DateTime.now().toIso8601String(),
        'attempts': 0,
      }),
    );
    await preferences.setStringList(_pendingKey, pending);
  }

  Future<void> _saveProducts(List<Product> products) async {
    final preferences = await _prefsFuture;
    await preferences.setString(
      _productsKey,
      jsonEncode(products.map(_toJson).toList()),
    );
  }

  Future<List<Product>> _readProducts() async {
    final preferences = await _prefsFuture;
    final raw = preferences.getString(_productsKey);
    if (raw == null) {
      return [];
    }
    return (jsonDecode(raw) as List<dynamic>)
        .map((item) => Product.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> _changeCachedQuantity(int productId, int change) async {
    final products = await _readProducts();
    final index = products.indexWhere((item) => item.id == productId);
    if (index < 0) return;
    final product = products[index];
    products[index] = product.withQuantity(
      (product.quantity + change).clamp(0, 2147483647).toInt(),
    );
    await _saveProducts(products);
  }

  Map<String, dynamic> _toJson(Product product) => {
    'id_produto': product.id,
    'nome_produto': product.name,
    'preco': product.price,
    'tipo': product.type,
    'quantidade_estoque': product.quantity,
    'codigo_barras': product.barcode,
    'descricao': product.description,
  };
}

class InventorySnapshot {
  const InventorySnapshot({
    required this.products,
    required this.isOffline,
    this.errorMessage,
  });
  final List<Product> products;
  final bool isOffline;
  final String? errorMessage;
}

class ExitResult {
  const ExitResult(this.success, this.message);
  final bool success;
  final String message;
}
