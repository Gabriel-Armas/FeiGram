package com.example.feigram.screens

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.Image
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
import coil.compose.rememberAsyncImagePainter
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.model.Profile
import com.example.feigram.network.model.posts.PostResponse
import com.example.feigram.screens.components.UserSession
import com.example.feigram.viewmodels.SessionViewModel
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(
    navController: NavController,
    sessionViewModel: SessionViewModel,
    profileId: String
) {
    val currentUser by sessionViewModel.userSession.collectAsState()
    val isMyProfile = currentUser?.userId == profileId

    val scope = rememberCoroutineScope()
    var userPosts by remember { mutableStateOf<List<PostResponse>>(emptyList()) }
    var publicacionSeleccionada by remember { mutableStateOf<String?>(null) }
    var loadError by remember { mutableStateOf<String?>(null) }
    var selectedImageUri by remember { mutableStateOf<Uri?>(null) }

    // Launcher para cambiar imagen de perfil
    val launcher = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->
        selectedImageUri = uri
        // Aquí podrías subir la imagen al backend
    }

    // Cargar publicaciones del usuario
    LaunchedEffect(profileId, currentUser) {
        try {
            loadError = null
            val posts = RetrofitInstance.postApi.getUserPosts(
                userId = profileId,
                token = "Bearer ${currentUser?.token.orEmpty()}"
            )
            userPosts = posts
        } catch (e: Exception) {
            e.printStackTrace()
            loadError = "Error al cargar publicaciones"
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = if (isMyProfile) "Mi perfil" else "Perfil de usuario") },
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
            // Imagen de perfil
            selectedImageUri?.let {
                Image(
                    painter = rememberAsyncImagePainter(it),
                    contentDescription = null,
                    modifier = Modifier
                        .size(160.dp)
                        .clip(CircleShape)
                        .border(2.dp, MaterialTheme.colorScheme.primary, CircleShape)
                )
            } ?: AsyncImage(
                model = currentUser?.profileImageUrl ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                contentDescription = "Foto de perfil",
                modifier = Modifier
                    .size(160.dp)
                    .clip(CircleShape)
                    .border(2.dp, MaterialTheme.colorScheme.primary, CircleShape)
            )

            Spacer(modifier = Modifier.height(16.dp))

            // Nombre, matrícula y correo
            Text(text = currentUser?.username ?: "Cargando...", style = MaterialTheme.typography.titleLarge)
            Text(text = currentUser?.enrollment ?: "SXXXXXX", style = MaterialTheme.typography.bodyMedium)
            Text(text = currentUser?.email ?: "Correo no disponible", style = MaterialTheme.typography.bodyMedium)

            Spacer(modifier = Modifier.height(16.dp))

            // Botón para cambiar foto si es mi perfil
            if (isMyProfile) {
                Button(onClick = { launcher.launch("image/*") }) {
                    Text("Cambiar imagen de perfil")
                }
            }

            Spacer(modifier = Modifier.height(16.dp))

            // Botón seguir o texto
            if (!isMyProfile) {
                Button(onClick = { /* TODO: Seguir usuario */ }) {
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

            if (loadError != null) {
                Text(text = loadError!!, color = MaterialTheme.colorScheme.error)
            } else {
                LazyVerticalGrid(
                    columns = GridCells.Fixed(3),
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(min = 300.dp, max = 600.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp),
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    userScrollEnabled = false,
                    content = {
                        items(userPosts) { post ->
                            AsyncImage(
                                model = post.urlMedia,
                                contentDescription = "Publicación de ${currentUser?.username}",
                                modifier = Modifier
                                    .aspectRatio(1f)
                                    .fillMaxWidth()
                                    .clickable {
                                        publicacionSeleccionada = post.urlMedia
                                    }
                            )
                        }
                    }
                )
            }
        }

        // Dialogo para ver la imagen seleccionada en grande
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
