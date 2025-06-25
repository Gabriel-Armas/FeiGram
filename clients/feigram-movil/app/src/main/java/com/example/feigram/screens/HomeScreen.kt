package com.example.feigram.screens

import androidx.compose.animation.core.FastOutSlowInEasing
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.scale
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.AsyncImage
import com.example.feigram.screens.components.PostItem
import com.example.feigram.viewmodels.SessionViewModel
import kotlinx.coroutines.launch

data class Comment(
    val username: String,
    val profileImageUrl: String,
    val text: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen(navController: NavController, sessionViewModel: SessionViewModel) {
    val drawerState = rememberDrawerState(DrawerValue.Closed)
    val scope = rememberCoroutineScope()

    val sheetState = rememberModalBottomSheetState(skipPartiallyExpanded = true)
    var showComments by remember { mutableStateOf(false) }
    var currentComments by remember { mutableStateOf<List<Comment>>(emptyList()) }
    var newComment by remember { mutableStateOf("") }

    var showLogoutDialog by remember { mutableStateOf(false) }

    val userSession by sessionViewModel.userSession.collectAsState()

    if (showLogoutDialog) {
        AlertDialog(
            onDismissRequest = { showLogoutDialog = false },
            title = { Text("Cerrar sesión") },
            text = { Text("¿Estás seguro de que deseas cerrar sesión?") },
            confirmButton = {
                TextButton(onClick = {
                    showLogoutDialog = false
                    navController.navigate("login") {
                        popUpTo("home") { inclusive = true }
                    }
                }) {
                    Text("Sí")
                }
            },
            dismissButton = {
                TextButton(onClick = { showLogoutDialog = false }) {
                    Text("Cancelar")
                }
            }
        )
    }

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            ModalDrawerSheet {
                Spacer(Modifier.height(12.dp))

                // Aquí agregamos la navegación real al perfil
                Text(
                    text = "Perfil (${userSession?.username ?: "Desconocido"})",
                    modifier = Modifier
                        .padding(16.dp)
                        .clickable {
                            userSession?.userId?.let { id ->
                                navController.navigate("profile/$id")
                            }
                            scope.launch { drawerState.close() }
                        }
                )

                Text(
                    "Ajustes",
                    modifier = Modifier
                        .padding(16.dp)
                        .clickable {
                            navController.navigate("settings")
                            scope.launch { drawerState.close() }
                        }
                )

                Text(
                    "Cerrar sesión",
                    modifier = Modifier
                        .padding(16.dp)
                        .clickable { showLogoutDialog = true }
                )
            }
        }
    ) {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = { Text("Feigram") },
                    navigationIcon = {
                        IconButton(onClick = {
                            scope.launch { drawerState.open() }
                        }) {
                            Icon(Icons.Default.Menu, contentDescription = "Menú")
                        }
                    },
                    actions = {
                        IconButton(onClick = {
                            println("Buscar usuario desde ícono")
                        }) {
                            Icon(Icons.Default.Search, contentDescription = "Buscar")
                        }
                    }
                )
            },
            floatingActionButton = {
                val infiniteTransition = rememberInfiniteTransition(label = "pulse")
                val scale by infiniteTransition.animateFloat(
                    initialValue = 1f,
                    targetValue = 1.1f,
                    animationSpec = infiniteRepeatable(
                        animation = tween(800, easing = FastOutSlowInEasing),
                        repeatMode = RepeatMode.Reverse
                    ), label = "scaleAnim"
                )

                FloatingActionButton(
                    onClick = {
                        navController.navigate("newpost")
                    },
                    modifier = Modifier
                        .padding(16.dp)
                        .scale(scale),
                    containerColor = MaterialTheme.colorScheme.primary
                ) {
                    Text("+", style = MaterialTheme.typography.titleLarge, color = MaterialTheme.colorScheme.onPrimary)
                }
            }
        ) { padding ->
            Box(modifier = Modifier.fillMaxSize()) {
                LazyColumn(
                    contentPadding = padding,
                    modifier = Modifier.fillMaxSize()
                ) {
                    items(3) { index ->
                        PostItem(
                            username = "usuario$index",
                            profileImageUrl = "https://randomuser.me/api/portraits/men/${index + 10}.jpg",
                            imageUrl = "https://res.cloudinary.com/demo/image/upload/sample.jpg",
                            description = "Publicación de ejemplo #$index",
                            comments = listOf("Comentario 1", "Comentario 2", "Comentario 3"),
                            date = "07 jun 2025",
                            onCommentsClick = {
                                currentComments = listOf(
                                    Comment("ana", "https://randomuser.me/api/portraits/women/1.jpg", "Comentario 1"),
                                    Comment("juan", "https://randomuser.me/api/portraits/men/2.jpg", "Comentario 2"),
                                    Comment("luz", "https://randomuser.me/api/portraits/women/3.jpg", "Comentario 3")
                                )
                                showComments = true
                            }
                        )
                    }
                }

                if (showComments) {
                    ModalBottomSheet(
                        onDismissRequest = {
                            showComments = false
                            newComment = ""
                        },
                        sheetState = sheetState
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp)
                        ) {
                            Text("Comentarios", style = MaterialTheme.typography.titleMedium)
                            Spacer(modifier = Modifier.height(8.dp))

                            currentComments.forEach { comment ->
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(vertical = 8.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    AsyncImage(
                                        model = comment.profileImageUrl,
                                        contentDescription = "Foto de ${comment.username}",
                                        modifier = Modifier
                                            .size(40.dp)
                                            .padding(end = 8.dp)
                                    )

                                    Column {
                                        Text(
                                            text = comment.username,
                                            style = MaterialTheme.typography.labelMedium
                                        )
                                        Text(text = comment.text)
                                    }
                                }
                            }

                            Spacer(modifier = Modifier.height(16.dp))

                            OutlinedTextField(
                                value = newComment,
                                onValueChange = { newComment = it },
                                label = { Text("Escribe un comentario...") },
                                modifier = Modifier.fillMaxWidth()
                            )

                            Spacer(modifier = Modifier.height(8.dp))

                            Button(
                                onClick = {
                                    if (newComment.isNotBlank()) {
                                        val fakeUser = Comment(
                                            username = userSession?.username ?: "yo",
                                            profileImageUrl = "https://randomuser.me/api/portraits/lego/1.jpg",
                                            text = newComment
                                        )
                                        currentComments = currentComments + fakeUser
                                        newComment = ""
                                    }
                                },
                                modifier = Modifier.align(Alignment.End)
                            ) {
                                Text("Enviar")
                            }
                        }
                    }
                }
            }
        }
    }
}
