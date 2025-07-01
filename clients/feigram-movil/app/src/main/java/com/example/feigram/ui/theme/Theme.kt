package com.example.feigram.ui.theme

import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext

// 🎨 Colores para modo claro
private val LightColorScheme = lightColorScheme(
    primary = Color(0xFF00796B),        // Verde fuerte
    secondary = Color(0xFF0288D1),      // Azul medio
    tertiary = Color(0xFF4CAF50),       // Verde más claro para acentos
    background = Color(0xFFFFFFFF),     // Blanco puro
    surface = Color(0xFFFFFFFF),
    error = Color(0xFFD32F2F),          // Rojo fuerte para errores
    onPrimary = Color.White,            // Texto sobre primary
    onSecondary = Color.White,
    onTertiary = Color.White,
    onBackground = Color.Black,
    onSurface = Color.Black,
    onError = Color.White
)

// 🎨 Colores para modo oscuro
private val DarkColorScheme = darkColorScheme(
    primary = Color(0xFF26A69A),        // Verde agua
    secondary = Color(0xFF29B6F6),      // Azul cielo
    tertiary = Color(0xFF81C784),       // Verde más pastel para acentos
    background = Color(0xFF121212),     // Fondo negro grisáceo
    surface = Color(0xFF1E1E1E),        // Superficies ligeramente más claras
    error = Color(0xFFEF5350),          // Rojo más pastel para errores
    onPrimary = Color.Black,
    onSecondary = Color.Black,
    onTertiary = Color.Black,
    onBackground = Color.White,
    onSurface = Color.White,
    onError = Color.Black
)

@Composable
fun FeigramTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    dynamicColor: Boolean = false,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context) else dynamicLightColorScheme(context)
        }
        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}
