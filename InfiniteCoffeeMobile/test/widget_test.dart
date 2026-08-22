import 'package:flutter_test/flutter_test.dart';

import 'package:infinite_coffee_app/main.dart';

void main() {
  testWidgets('apresenta a navegacao principal do Infinite Coffee', (
    tester,
  ) async {
    await tester.pumpWidget(const InfiniteCoffeeApp());

    expect(find.text('Início'), findsOneWidget);
    expect(find.text('Produtos'), findsOneWidget);
    expect(find.text('Estoque'), findsOneWidget);
  });
}
