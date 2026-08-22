import 'package:flutter/material.dart';

class AppTheme {
  static ThemeData get light {
    const coffee = Color(0xFFB5541A);
    const deepCoffee = Color(0xFF2C1A0E);
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: coffee,
        surface: Colors.white,
      ),
      scaffoldBackgroundColor: Colors.white,
      fontFamily: 'sans',
      appBarTheme: const AppBarTheme(
        backgroundColor: coffee,
        foregroundColor: Colors.white,
      ),
      inputDecorationTheme: InputDecorationTheme(
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide(color: coffee, width: 2),
        ),
      ),
      textTheme: const TextTheme(
        headlineMedium: TextStyle(
          color: deepCoffee,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }
}
