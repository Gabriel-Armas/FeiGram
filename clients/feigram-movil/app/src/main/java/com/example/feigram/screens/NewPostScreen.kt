package com.example.feigram.screens

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NewPostScreen(navController: NavController) {
    var imageUri by remember { mutableStateOf<Uri?>(null) }
    var description by remember { mutableStateOf("") }

    val galleryLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri: Uri? ->
        imageUri = uri
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Nueva publicación") },
                actions = {
                    IconButton(
                        onClick = {
                            println("Descripción: $description")
                            println("Imagen: $imageUri")
                            navController.popBackStack()
                        },
                        enabled = imageUri != null && description.isNotBlank()
                    ) {
                        Icon(Icons.Default.Check, contentDescription = "Publicar")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .padding(padding)
                .padding(16.dp)
                .fillMaxSize(),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Button(onClick = { galleryLauncher.launch("image/*") }) {
                Text("Seleccionar imagen")
            }

            imageUri?.let {
                Image(
                    painter = rememberAsyncImagePainter(it),
                    contentDescription = "Imagen seleccionada",
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(250.dp),
                    contentScale = ContentScale.Crop
                )
            }

            OutlinedTextField(
                value = description,
                onValueChange = { description = it },
                label = { Text("Descripción") },
                modifier = Modifier.fillMaxWidth()
            )
        }
    }
}
