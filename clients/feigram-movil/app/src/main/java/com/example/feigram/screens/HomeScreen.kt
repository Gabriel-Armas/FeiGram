package com.example.feigram.screens

import androidx.compose.animation.core.*
import androidx.compose.foundation.Image
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.*
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Block
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.scale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.AsyncImage
import com.example.feigram.R
import com.example.feigram.network.model.Profile
import com.example.feigram.network.model.comments.Comment
import com.example.feigram.network.model.feed.FeedPost
import com.example.feigram.network.model.likes.LikeRequest
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.screens.components.PostItem
import com.example.feigram.viewmodels.SessionViewModel
import kotlinx.coroutines.launch

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
    var feedPosts by remember { mutableStateOf<List<FeedPost>>(emptyList()) }
    var isLoading by remember { mutableStateOf(false) }
    var currentPostId by remember { mutableStateOf<Int?>(null) }

    var skip by remember { mutableStateOf(0) }
    val listState = rememberLazyListState()

    var firstLoadDone by remember { mutableStateOf(false) }

    val isAdmin = userSession?.rol.equals("Admin", ignoreCase = true)

    LaunchedEffect(userSession) {
        println("ROL DEL USUARIO AL INICIAR HOME: ${userSession?.rol}")
        if (userSession != null) {
            isLoading = true
            skip = 0
            try {
                val response = RetrofitInstance.feedApi.getRecommendations(
                    token = "Bearer ${userSession!!.token}",
                    userId = userSession!!.userId
                )
                feedPosts = response.posts
            } catch (e: Exception) {
                e.printStackTrace()
            } finally {
                isLoading = false
                firstLoadDone = true
            }
        }
    }

    LaunchedEffect(listState, feedPosts.size) {
        snapshotFlow {
            listState.layoutInfo.visibleItemsInfo.lastOrNull()?.index
        }.collect { lastVisibleIndex ->
            if (lastVisibleIndex != null && lastVisibleIndex >= feedPosts.size - 1 && !isLoading) {
                isLoading = true
                skip += 10
                try {
                    val newResponse = RetrofitInstance.feedApi.getRecommendations(
                        token = "Bearer ${userSession?.token.orEmpty()}",
                        userId = userSession?.userId ?: "",
                        skip = skip
                    )
                    feedPosts = feedPosts + newResponse.posts
                } catch (e: Exception) {
                    e.printStackTrace()
                } finally {
                    isLoading = false
                }
            }
        }
    }

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
                }) { Text("Sí") }
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
            ModalDrawerSheet(
                modifier = Modifier.fillMaxHeight()
            ) {
                Spacer(Modifier.height(16.dp))

                Text(
                    text = "Menú",
                    style = MaterialTheme.typography.titleLarge,
                    modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
                )

                HorizontalDivider()

                ListItem(
                    headlineContent = { Text("Mi perfil") },
                    leadingContent = { Icon(Icons.Default.AccountCircle, contentDescription = "Perfil") },
                    modifier = Modifier.clickable {
                        scope.launch {
                            userSession?.userId?.let { id ->
                                navController.navigate("profile/$id")
                            }
                            drawerState.close()
                        }
                    }
                )

                ListItem(
                    headlineContent = { Text("Cerrar sesión") },
                    leadingContent = { Icon(Icons.AutoMirrored.Filled.ExitToApp, contentDescription = "Cerrar sesión") },
                    modifier = Modifier.clickable {
                        scope.launch {
                            showLogoutDialog = true
                            drawerState.close()
                        }
                    }
                )

                if (isAdmin) {
                    ListItem(
                        headlineContent = { Text("Gestión de cuentas") },
                        leadingContent = { Icon(Icons.Default.Block, contentDescription = "Admin") },
                        modifier = Modifier.clickable {
                            scope.launch {
                                navController.navigate("adminAccounts")
                                drawerState.close()
                            }
                        }
                    )
                }

                HorizontalDivider()

                ListItem(
                    headlineContent = { Text("Mensajes") },
                    leadingContent = { Icon(Icons.AutoMirrored.Filled.Send, contentDescription = "Mensajes") },
                    modifier = Modifier.clickable {
                        scope.launch {
                            navController.navigate("messages")
                            drawerState.close()
                        }
                    }
                )
            }
        }
    )
    {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = {
                        Image(
                            painter = painterResource(id = R.drawable.feigram_logofont),
                            contentDescription = "Logo Feigram",
                            modifier = Modifier
                                .height(32.dp)
                        )
                    },
                    navigationIcon = {
                        IconButton(onClick = { scope.launch { drawerState.open() } }) {
                            Icon(Icons.Default.Menu, contentDescription = "Menú")
                        }
                    },
                    actions = {
                        IconButton(onClick = { navController.navigate("usersearch") }) {
                            Icon(Icons.Default.Search, contentDescription = "Buscar usuario")
                        }
                        IconButton(onClick = {
                            navController.navigate("messages")
                        }) {
                            Icon(Icons.AutoMirrored.Filled.Send, contentDescription = "Mensajes")
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
                    Icon(Icons.Default.Add, contentDescription = "Nuevo post", tint = MaterialTheme.colorScheme.onPrimary)
                }
            }
        ) { padding ->
            Box(modifier = Modifier.fillMaxSize()) {
                if (!firstLoadDone) {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                } else {
                    LazyColumn(
                        contentPadding = padding,
                        state = listState,
                        modifier = Modifier.fillMaxSize(),
                        verticalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        items(feedPosts) { post ->
                            var profile by remember { mutableStateOf<Profile?>(null) }
                            var likeCount by remember { mutableStateOf(post.likes) }
                            var commentCount by remember { mutableStateOf(post.comentarios) }
                            var hasLiked by remember { mutableStateOf(false) }

                            LaunchedEffect(post.id_usuario) {
                                try {
                                    profile = RetrofitInstance.profileApi.getProfileById(
                                        id = post.id_usuario,
                                        token = "Bearer ${userSession?.token.orEmpty()}"
                                    )

                                    hasLiked = RetrofitInstance.likeApi.checkLike(
                                        userId = userSession?.userId ?: "",
                                        postId = post.post_id.toString(),
                                        token = "Bearer ${userSession?.token.orEmpty()}"
                                    )

                                } catch (e: Exception) {
                                    e.printStackTrace()
                                }
                            }

                            Column(modifier = Modifier.padding(8.dp)) {
                                PostItem(
                                    username = profile?.name ?: "Cargando...",
                                    profileImageUrl = profile?.photo ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                                    imageUrl = post.url_media,
                                    description = post.descripcion,
                                    date = post.fechaPublicacion.substring(0, 10),
                                    commentCount = commentCount,
                                    likeCount = likeCount,
                                    onCommentsClick = {
                                        scope.launch {
                                            try {
                                                currentPostId = post.post_id

                                                val response = RetrofitInstance.postApi.getPostComments(
                                                    postId = post.post_id,
                                                    token = "Bearer ${userSession?.token.orEmpty()}"
                                                )

                                                val commentsWithUsernames = response.comments.map { c ->
                                                    val profile = try {
                                                        RetrofitInstance.profileApi.getProfileById(
                                                            id = c.userId,
                                                            token = "Bearer ${userSession?.token.orEmpty()}"
                                                        )
                                                    } catch (e: Exception) {
                                                        null
                                                    }

                                                    Comment(
                                                        username = profile?.name ?: c.userId,
                                                        profileImageUrl = profile?.photo ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                                                        text = c.textComment ?: ""
                                                    )
                                                }

                                                currentComments = commentsWithUsernames
                                                showComments = true

                                            } catch (e: Exception) {
                                                e.printStackTrace()
                                            }
                                        }
                                    },
                                    onLikeClick = {
                                        scope.launch {
                                            try {
                                                if (hasLiked) {
                                                    RetrofitInstance.likeApi.deleteLike(
                                                        userId = userSession?.userId ?: "",
                                                        postId = post.post_id.toString(),
                                                        token = "Bearer ${userSession?.token.orEmpty()}"
                                                    )
                                                    likeCount--
                                                } else {
                                                    RetrofitInstance.likeApi.addLike(
                                                        like = LikeRequest(
                                                            userId = userSession?.userId ?: "",
                                                            postId = post.post_id.toString()
                                                        ),
                                                        token = "Bearer ${userSession?.token.orEmpty()}"
                                                    )
                                                    likeCount++
                                                }
                                                hasLiked = !hasLiked
                                            } catch (e: Exception) {
                                                e.printStackTrace()
                                            }
                                        }
                                    },
                                    hasLiked = hasLiked
                                )

                                HorizontalDivider(
                                    modifier = Modifier.padding(vertical = 4.dp),
                                    thickness = 0.5.dp,
                                    color = MaterialTheme.colorScheme.outline.copy(alpha = 0.3f)
                                )
                            }
                        }

                        item {
                            if (isLoading) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(16.dp),
                                    contentAlignment = Alignment.Center
                                ) {
                                    CircularProgressIndicator()
                                }
                            }
                        }
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
                                    verticalAlignment = Alignment.Top
                                ) {
                                    AsyncImage(
                                        model = comment.profileImageUrl,
                                        contentDescription = "Foto de ${comment.username}",
                                        modifier = Modifier
                                            .size(36.dp)
                                            .clip(CircleShape)
                                            .padding(end = 8.dp)
                                    )

                                    Column(  // <-- Aquí antes no tenías modifiers
                                        modifier = Modifier.fillMaxWidth()
                                    ) {
                                        Text(
                                            comment.username,
                                            style = MaterialTheme.typography.labelLarge,
                                            color = MaterialTheme.colorScheme.primary
                                        )
                                        Text(
                                            comment.text,
                                            style = MaterialTheme.typography.bodyMedium
                                        )
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
                                    if (newComment.isNotBlank() && currentPostId != null) {
                                        scope.launch {
                                            try {
                                                RetrofitInstance.commentApi.addComment(
                                                    comment = com.example.feigram.network.model.comments.CommentRequest(
                                                        userId = userSession?.userId ?: "",
                                                        postId = currentPostId.toString(),
                                                        textComment = newComment,
                                                        createdAt = java.time.Instant.now().toString()
                                                    ),
                                                    token = "Bearer ${userSession?.token.orEmpty()}"
                                                )

                                                val response = RetrofitInstance.postApi.getPostComments(
                                                    postId = currentPostId!!,
                                                    token = "Bearer ${userSession?.token.orEmpty()}"
                                                )

                                                val commentsWithProfiles = response.comments.map { c ->
                                                    val profile = try {
                                                        RetrofitInstance.profileApi.getProfileById(
                                                            id = c.userId,
                                                            token = "Bearer ${userSession?.token.orEmpty()}"
                                                        )
                                                    } catch (e: Exception) {
                                                        null
                                                    }

                                                    Comment(
                                                        username = profile?.name ?: c.userId,
                                                        profileImageUrl = profile?.photo ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                                                        text = c.textComment
                                                    )
                                                }

                                                currentComments = commentsWithProfiles
                                                newComment = ""

                                            } catch (e: Exception) {
                                                e.printStackTrace()
                                            }
                                        }
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
