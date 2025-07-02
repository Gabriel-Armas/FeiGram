package com.example.feigram.screens.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.FavoriteBorder
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
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
    hasLiked: Boolean,
    onLikeClick: () -> Unit,
    onCommentsClick: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(bottom = 8.dp)
    ) {
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

        AsyncImage(
            model = imageUrl,
            contentDescription = "Imagen publicada",
            modifier = Modifier
                .fillMaxWidth()
                .height(500.dp),
            contentScale = ContentScale.Crop
        )

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp, vertical = 4.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                IconButton(onClick = onLikeClick) {
                    Icon(
                        imageVector = if (hasLiked) Icons.Default.Favorite else Icons.Default.FavoriteBorder,
                        contentDescription = if (hasLiked) "Quitar Like" else "Dar Like",
                        tint = if (hasLiked) Color.Red else Color.Gray
                    )
                }
                Text(text = "$likeCount likes", style = MaterialTheme.typography.labelMedium)
            }

            TextButton(onClick = onCommentsClick) {
                Text(text = "Ver comentarios ($commentCount)")
            }
        }

        Text(
            text = description,
            modifier = Modifier
                .padding(horizontal = 12.dp, vertical = 4.dp),
            style = MaterialTheme.typography.bodyMedium
        )
    }
}
