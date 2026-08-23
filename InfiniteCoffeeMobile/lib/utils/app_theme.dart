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
      scaffoldBackgroundColor: Color(0xFFF8F4F0),
      fontFamily: 'sans',
      appBarTheme: const AppBarTheme(
        backgroundColor: coffee,
        foregroundColor: Colors.white,
        elevation: 0,
        titleTextStyle: TextStyle(
          color: Colors.white,
          fontSize: 22,
          fontWeight: FontWeight.w700,
        ),
      ),
      navigationRailTheme: const NavigationRailThemeData(
        backgroundColor: Colors.transparent,
        selectedIconTheme: IconThemeData(color: coffee),
        selectedLabelTextStyle: TextStyle(
          color: deepCoffee,
          fontWeight: FontWeight.w700,
        ),
        unselectedLabelTextStyle: TextStyle(color: Colors.black54),
      ),
      cardTheme: CardThemeData(
        color: Colors.white,
        elevation: 1,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(14),
          side: const BorderSide(color: Color(0xFFE9DED3)),
        ),
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
