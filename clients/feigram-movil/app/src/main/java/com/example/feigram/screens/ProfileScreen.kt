package com.example.feigram.screens

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.AsyncImage
import com.example.feigram.screens.components.UserSession
import com.example.feigram.viewmodels.SessionViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(
    navController: NavController,
    sessionViewModel: SessionViewModel,
    profileId: String
) {
    val currentUser by sessionViewModel.userSession.collectAsState()
    val isMyProfile = currentUser?.userId == profileId

    val userProfile = if (currentUser != null && isMyProfile) {
        UserSession(
            userId = currentUser!!.userId,
            username = currentUser!!.username,
            matricula = currentUser!!.matricula,
            email = "example1@gmail.com",
            profileImageUrl = currentUser!!.profileImageUrl
        )
    } else {
        UserSession(
            userId = profileId,
            username = "Otro Usuario",
            matricula = "A00123456",
            email = "example2@gmail.com",
            profileImageUrl = "https://randomuser.me/api/portraits/men/5.jpg"
        )
    }

    val publicacionesUrls = listOf(
        "https://picsum.photos/id/1011/300/300",
        "https://picsum.photos/id/1025/300/300",
        "https://picsum.photos/id/1035/300/300",
        "https://picsum.photos/id/1043/300/300",
        "https://picsum.photos/id/106/300/300",
        "https://picsum.photos/id/107/300/300"
    )

    val seguidores = if (isMyProfile) 128 else 54

    // Estado para la publicación seleccionada (url de la imagen)
    var publicacionSeleccionada by remember { mutableStateOf<String?>(null) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = if (isMyProfile) "Mi perfil" else userProfile.username) },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        }
    ) { padding ->
        // Scroll vertical para todo el contenido
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(24.dp)
                .verticalScroll(rememberScrollState()),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            AsyncImage(
                model = userProfile.profileImageUrl,
                contentDescription = "Foto de perfil",
                modifier = Modifier.size(120.dp)
            )
            Spacer(modifier = Modifier.height(16.dp))

            Text(text = userProfile.username, style = MaterialTheme.typography.titleLarge)
            Text(text = userProfile.matricula, style = MaterialTheme.typography.bodyMedium)

            Spacer(modifier = Modifier.height(8.dp))

            Text(text = "Seguidores: $seguidores", style = MaterialTheme.typography.bodyMedium)

            Spacer(modifier = Modifier.height(24.dp))

            if (!isMyProfile) {
                Button(onClick = {
                    // Aquí lógica para seguir
                }) {
                    Text("Seguir")
                }
            } else {
                Text(
                    text = "Este es tu perfil",
                    style = MaterialTheme.typography.labelMedium,
                    color = MaterialTheme.colorScheme.primary
                )
            }

            Spacer(modifier = Modifier.height(24.dp))

            Text("Publicaciones", style = MaterialTheme.typography.titleMedium)
            Spacer(modifier = Modifier.height(8.dp))

            // Grid con 3 columnas para mostrar imágenes
            // El grid estará contenido dentro de un Column con scroll general
            LazyVerticalGrid(
                columns = GridCells.Fixed(3),
                modifier = Modifier
                    .fillMaxWidth()
                    .heightIn(min = 300.dp, max = 600.dp), // para evitar conflicto con scroll
                verticalArrangement = Arrangement.spacedBy(8.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                userScrollEnabled = false, // deshabilitamos scroll interno para evitar conflicto
                content = {
                    items(publicacionesUrls) { url ->
                        AsyncImage(
                            model = url,
                            contentDescription = "Publicación de ${userProfile.username}",
                            modifier = Modifier
                                .aspectRatio(1f) // cuadrado
                                .fillMaxWidth()
                                .clickable {
                                    publicacionSeleccionada = url // mostrar detalle al hacer clic
                                }
                        )
                    }
                }
            )
        }

        // Dialogo para mostrar detalles de la publicación seleccionada
        if (publicacionSeleccionada != null) {
            AlertDialog(
                onDismissRequest = { publicacionSeleccionada = null },
                confirmButton = {
                    TextButton(onClick = { publicacionSeleccionada = null }) {
                        Text("Cerrar")
                    }
                }, text = {
                    AsyncImage(
                        model = publicacionSeleccionada,
                        contentDescription = "Detalle publicación",
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            )
        }
    }
}
