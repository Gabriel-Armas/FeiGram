package com.example.feigram.ui.theme

import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext

// Colores base: Verde, Azul, Blanco
private val LightColorScheme = lightColorScheme(
    primary = Color(0xFF00796B),        // Verde fuerte
    secondary = Color(0xFF0288D1),      // Azul intermedio
    tertiary = Color(0xFF4CAF50),       // Verde más claro (para acentos)
    background = Color(0xFFFFFFFF),     // Blanco total
    surface = Color(0xFFFFFFFF),
    onPrimary = Color.White,            // Texto blanco sobre primary
    onSecondary = Color.White,          // Texto blanco sobre secondary
    onTertiary = Color.White,
    onBackground = Color.Black,         // Texto negro sobre fondo claro
    onSurface = Color.Black
)

private val DarkColorScheme = darkColorScheme(
    primary = Color(0xFF80CBC4),        // Verde suave (modo oscuro)
    secondary = Color(0xFF81D4FA),      // Azul claro (modo oscuro)
    tertiary = Color(0xFF66BB6A),       // Verde extra para acentos
    background = Color(0xFF121212),     // Fondo gris oscuro
    surface = Color(0xFF1E1E1E),
    onPrimary = Color.Black,            // Texto negro sobre verde claro
    onSecondary = Color.Black,          // Texto negro sobre azul claro
    onTertiary = Color.Black,
    onBackground = Color.White,         // Texto blanco sobre fondo oscuro
    onSurface = Color.White
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
