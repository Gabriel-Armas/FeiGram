package com.example.feigram.screens.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.FavoriteBorder
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage

@Composable
fun PostItem(
    username: String,
    profileImageUrl: String,
    imageUrl: String,
    description: String,
    date: String,
    commentCount: Int,
    likeCount: Int,
    onCommentsClick: () -> Unit
) {
    var isLiked by remember { mutableStateOf(false) }

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(bottom = 8.dp)  // 👈 Solo un pequeño espacio entre posts
    ) {
        // Header del usuario
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .padding(horizontal = 12.dp, vertical = 8.dp)
                .fillMaxWidth()
        ) {
            AsyncImage(
                model = profileImageUrl,
                contentDescription = "Foto de perfil de $username",
                modifier = Modifier
                    .size(40.dp)
                    .padding(end = 8.dp)
            )
            Column {
                Text(text = username, style = MaterialTheme.typography.labelLarge)
                Text(text = date, style = MaterialTheme.typography.labelSmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
            }
        }

        // Imagen del post (más alargada)
        AsyncImage(
            model = imageUrl,
            contentDescription = "Imagen publicada",
            modifier = Modifier
                .fillMaxWidth()
                .height(500.dp),  // 👈 Altura más larga
            contentScale = ContentScale.Crop
        )

        // Likes y comentarios
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp, vertical = 4.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            IconButton(onClick = { isLiked = !isLiked }) {
                Icon(
                    imageVector = if (isLiked) Icons.Filled.Favorite else Icons.Default.FavoriteBorder,
                    contentDescription = if (isLiked) "Deshacer like" else "Dar like",
                    tint = if (isLiked) Color.Red else LocalContentColor.current
                )
            }

            Text(
                text = "$likeCount Me gusta • $commentCount comentarios",
                style = MaterialTheme.typography.bodySmall
            )
        }

        // Descripción
        Text(
            text = description,
            modifier = Modifier
                .padding(horizontal = 12.dp, vertical = 4.dp),
            style = MaterialTheme.typography.bodyMedium
        )

        // Ver comentarios
        Text(
            text = "Ver comentarios",
            color = MaterialTheme.colorScheme.primary,
            style = MaterialTheme.typography.bodySmall,
            modifier = Modifier
                .padding(horizontal = 12.dp, vertical = 4.dp)
                .clickable { onCommentsClick() }
        )
    }
}
