package com.example.feigram.screens

import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
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
            enrollment = currentUser!!.enrollment,
            email = "example1@gmail.com",
            profileImageUrl = currentUser!!.profileImageUrl,
            rol = currentUser!!.rol
        )
    } else {
        UserSession(
            userId = profileId,
            username = "Desconocido",
            enrollment = "SXXXXXX",
            email = "No disponible",
            profileImageUrl = "https://th.bing.com/th/id/OIP.ccRFOtJyhtb9QxwnH3N89wHaHa?rs=1&pid=ImgDetMain&cb=idpwebp2&o=7&rm=3",
            rol = "Banned"
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
                modifier = Modifier
                    .size(160.dp)
                    .clip(CircleShape)
                    .border(2.dp, MaterialTheme.colorScheme.primary, CircleShape)
            )
            Spacer(modifier = Modifier.height(16.dp))

            Text(text = userProfile.username, style = MaterialTheme.typography.titleLarge)
            Text(text = userProfile.enrollment, style = MaterialTheme.typography.bodyMedium)

            Spacer(modifier = Modifier.height(8.dp))

            Text(text = "Seguidores: $seguidores", style = MaterialTheme.typography.bodyMedium)

            Spacer(modifier = Modifier.height(24.dp))

            if (!isMyProfile) {
                Button(onClick = {
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

            LazyVerticalGrid(
                columns = GridCells.Fixed(3),
                modifier = Modifier
                    .fillMaxWidth()
                    .heightIn(min = 300.dp, max = 600.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                userScrollEnabled = false,
                content = {
                    items(publicacionesUrls) { url ->
                        AsyncImage(
                            model = url,
                            contentDescription = "Publicación de ${userProfile.username}",
                            modifier = Modifier
                                .aspectRatio(1f)
                                .fillMaxWidth()
                                .clickable {
                                    publicacionSeleccionada = url
                                }
                        )
                    }
                }
            )
        }

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
