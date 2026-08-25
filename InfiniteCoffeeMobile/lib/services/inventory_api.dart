import 'dart:convert';
import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

import '../models/product.dart';

class InventoryApi {
  InventoryApi({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;
  static const _configuredBaseUrl = String.fromEnvironment('API_BASE_URL');
  static const _apiToken = String.fromEnvironment('API_ACCESS_TOKEN');
  static const _writeToken = String.fromEnvironment('API_WRITE_TOKEN');
  static const _productionBaseUrl =
      'https://padaria-debortolo-api-8w5w.onrender.com';

  Map<String, String> _headers([
    Map<String, String>? extra,
    bool write = false,
  ]) {
    final token = write && _writeToken.trim().isNotEmpty
        ? _writeToken
        : _apiToken;
    return {if (token.trim().isNotEmpty) 'X-Api-Key': token, ...?extra};
  }

  // API_BASE_URL permite apontar para o backend local durante o desenvolvimento.
  String get baseUrl {
    if (_configuredBaseUrl.trim().isNotEmpty) {
      return _configuredBaseUrl.replaceFirst(RegExp(r'/$'), '');
    }
    if (kIsWeb) return 'http://localhost:5049';
    return defaultTargetPlatform == TargetPlatform.android
        ? _productionBaseUrl
        : 'http://localhost:5049';
  }

  Future<List<Product>> getStock({String search = ''}) async {
    final uri = Uri.parse('$baseUrl/api/estoque').replace(
      queryParameters: search.trim().isEmpty ? null : {'busca': search.trim()},
    );
    final response = await _client
        .get(uri, headers: _headers())
        .timeout(const Duration(seconds: 60));
    if (response.statusCode != 200) {
      if (response.statusCode == 401 || response.statusCode == 403) {
        throw ApiException(
          'A API recusou o acesso. Gere o APK com o token de leitura correto.',
          response.statusCode,
        );
      }
      throw ApiException(
        'A API retornou o erro ${response.statusCode}.',
        response.statusCode,
      );
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
          headers: _headers({'Content-Type': 'application/json'}, true),
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
        response.statusCode,
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
          headers: _headers({'Content-Type': 'application/json'}, true),
          body: jsonEncode({
            'produtoId': productId,
            'quantidade': quantity,
            'motivo': reason,
          }),
        )
        .timeout(const Duration(seconds: 8));
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw ApiException(
        'Nao foi possivel registrar a entrada.',
        response.statusCode,
      );
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
          headers: _headers({'Content-Type': 'application/json'}, true),
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
  const ApiException(this.message, [this.statusCode]);
  final String message;
  final int? statusCode;
}
