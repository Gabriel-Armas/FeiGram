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
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage

@Composable
fun PostItem(
    username: String,
    profileImageUrl: String,
    imageUrl: String,
    description: String,
    comments: List<String>,
    onCommentsClick: () -> Unit
) {
    var isLiked by remember { mutableStateOf(false) }

    Column(modifier = Modifier.fillMaxWidth()) {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier
                .padding(horizontal = 16.dp, vertical = 8.dp)
                .fillMaxWidth()
        ) {
            AsyncImage(
                model = profileImageUrl,
                contentDescription = "Foto de perfil de $username",
                modifier = Modifier
                    .size(40.dp)
                    .padding(end = 8.dp)
            )
            Text(
                text = username,
                style = MaterialTheme.typography.labelLarge
            )
        }

        // Imagen de la publicación
        AsyncImage(
            model = imageUrl,
            contentDescription = "Imagen publicada",
            modifier = Modifier
                .fillMaxWidth()
                .height(250.dp)
        )

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 16.dp, vertical = 8.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            IconButton(onClick = { isLiked = !isLiked }) {
                Icon(
                    imageVector = if (isLiked) Icons.Filled.Favorite else Icons.Default.FavoriteBorder,
                    contentDescription = if (isLiked) "Deshacer like" else "Dar like",
                    tint = if (isLiked) {
                        Color.Red
                    } else LocalContentColor.current
                )
            }

            Text(
                text = "Ver comentarios (${comments.size})",
                modifier = Modifier.clickable { onCommentsClick() },
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.primary
            )
        }

        Text(
            text = description,
            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
        )

        HorizontalDivider()
    }
}
