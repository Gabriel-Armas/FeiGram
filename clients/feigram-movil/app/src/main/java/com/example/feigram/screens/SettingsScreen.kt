package com.example.feigram.screens

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts.GetContent
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.AsyncImage
import coil.compose.rememberAsyncImagePainter
import coil.request.ImageRequest
import com.example.feigram.viewmodels.SessionViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    navController: NavController,
    sessionViewModel: SessionViewModel
) {
    val currentUser by sessionViewModel.userSession.collectAsState()
    var selectedImageUri by remember { mutableStateOf<Uri?>(null) }

    // Abrir selector de imagen
    val launcher = rememberLauncherForActivityResult(GetContent()) { uri: Uri? ->
        selectedImageUri = uri
        // Aquí puedes subir la imagen a tu backend o Firebase y guardar el nuevo URL
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Ajustes de perfil") },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text("Editar perfil", style = MaterialTheme.typography.titleLarge)
            Spacer(modifier = Modifier.height(16.dp))

            // Imagen de perfil actual o seleccionada
            selectedImageUri?.let {
                Image(
                    painter = rememberAsyncImagePainter(it),
                    contentDescription = null,
                    modifier = Modifier.size(120.dp)
                )
            } ?: AsyncImage(
                model = currentUser?.profileImageUrl,
                contentDescription = "Imagen de perfil",
                modifier = Modifier.size(120.dp)
            )

            Spacer(modifier = Modifier.height(8.dp))

            Button(onClick = {
                launcher.launch("image/*")
            }) {
                Text("Cambiar imagen de perfil")
            }

            Spacer(modifier = Modifier.height(32.dp))

            currentUser?.let {
                Text("Nombre: ${it.username}")
                Text("Matrícula: ${it.matricula}")
                Text("Correo: ${it.email}")
            }
        }
    }
}
