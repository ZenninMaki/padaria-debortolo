import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

import '../models/product.dart';

class InventoryApi {
  InventoryApi({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  // No emulador Android, localhost aponta para o proprio aparelho.
  // 10.0.2.2 redireciona para o computador que esta executando o backend.
  String get baseUrl {
    if (kIsWeb) return 'http://localhost:5049';
    return defaultTargetPlatform == TargetPlatform.android
        ? 'http://10.0.2.2:5049'
        : 'http://localhost:5049';
  }

  Future<List<Product>> getStock({String search = ''}) async {
    final uri = Uri.parse('$baseUrl/api/estoque').replace(
      queryParameters: search.trim().isEmpty ? null : {'busca': search.trim()},
    );
    final response = await _client.get(uri).timeout(const Duration(seconds: 8));
    if (response.statusCode != 200) {
      throw const ApiException('Falha ao consultar estoque.');
    }
    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((item) => Product.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> registerExit({
    required int productId,
    required int quantity,
    required String reason,
  }) async {
    final response = await _client
        .post(
          Uri.parse('$baseUrl/api/estoque/saida'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'produtoId': productId,
            'quantidade': quantity,
            'motivo': reason,
          }),
        )
        .timeout(const Duration(seconds: 8));
    if (response.statusCode < 200 || response.statusCode >= 300) {
      final body = jsonDecode(response.body) as Map<String, dynamic>;
      throw ApiException(
        '${body['mensagem'] ?? 'Nao foi possivel registrar a saida.'}',
      );
    }
  }

  Future<void> registerEntry({
    required int productId,
    required int quantity,
    required String reason,
  }) async {
    final response = await _client
        .post(
          Uri.parse('$baseUrl/api/estoque/entrada'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'produtoId': productId,
            'quantidade': quantity,
            'motivo': reason,
          }),
        )
        .timeout(const Duration(seconds: 8));
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw const ApiException('Nao foi possivel registrar a entrada.');
    }
  }

  Future<void> createProduct({
    required String name,
    required String description,
    required String barcode,
    required String type,
    required double price,
    required int quantity,
  }) async {
    final response = await _client
        .post(
          Uri.parse('$baseUrl/api/produtos'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'nome': name,
            'descricao': description,
            'codigoBarras': barcode,
            'tipo': type,
            'preco': price,
            'quantidade': quantity,
          }),
        )
        .timeout(const Duration(seconds: 8));
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw const ApiException('Nao foi possivel cadastrar o produto.');
    }
  }
}

class ApiException implements Exception {
  const ApiException(this.message);
  final String message;
}
