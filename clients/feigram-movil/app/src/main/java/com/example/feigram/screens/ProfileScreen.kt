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
import androidx.compose.material.icons.automirrored.filled.Send
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.compose.AsyncImage
import coil.compose.rememberAsyncImagePainter
import com.example.feigram.network.model.posts.PostResponse
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.viewmodels.SessionViewModel
import kotlinx.coroutines.launch
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.toRequestBody

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
    var selectedPost by remember { mutableStateOf<PostResponse?>(null) }
    var loadError by remember { mutableStateOf<String?>(null) }
    var selectedImageUri by remember { mutableStateOf<Uri?>(null) }
    var isUploading by remember { mutableStateOf(false) }

    val context = LocalContext.current

    val launcher = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->
        if (uri != null) {
            selectedImageUri = uri
            scope.launch {
                try {
                    isUploading = true
                    val inputStream = context.contentResolver.openInputStream(uri)
                    val bytes = inputStream?.readBytes()
                    inputStream?.close()

                    val requestBody = bytes?.toRequestBody("image/*".toMediaTypeOrNull())
                    val multipart = requestBody?.let {
                        MultipartBody.Part.createFormData("photo", "profile.jpg", it)
                    }

                    if (multipart != null) {
                        sessionViewModel.updateProfilePhoto(
                            profileId = profileId,
                            photoPart = multipart,
                            onError = { e -> e.printStackTrace() }
                        )
                    }

                } catch (e: Exception) {
                    e.printStackTrace()
                } finally {
                    isUploading = false
                }
            }
        }
    }

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
                title = { Text(if (isMyProfile) "Mi perfil" else "Perfil de usuario") },
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
            Box(
                modifier = Modifier
                    .size(160.dp)
                    .clip(CircleShape)
                    .border(2.dp, MaterialTheme.colorScheme.primary, CircleShape)
                    .clickable(enabled = isMyProfile && !isUploading) {
                        launcher.launch("image/*")
                    },
                contentAlignment = Alignment.Center
            ) {
                selectedImageUri?.let {
                    Image(
                        painter = rememberAsyncImagePainter(it),
                        contentDescription = null,
                        modifier = Modifier.fillMaxSize()
                    )
                } ?: AsyncImage(
                    model = currentUser?.profileImageUrl ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                    contentDescription = "Foto de perfil",
                    modifier = Modifier.fillMaxSize()
                )

                if (isMyProfile) {
                    Box(
                        modifier = Modifier.matchParentSize(),
                        contentAlignment = Alignment.BottomEnd
                    ) {
                        Icon(
                            imageVector = Icons.Filled.Edit,
                            contentDescription = "Cambiar foto",
                            tint = MaterialTheme.colorScheme.primary,
                            modifier = Modifier.size(32.dp).padding(8.dp)
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.height(16.dp))

            Text(text = currentUser?.username ?: "Cargando...", style = MaterialTheme.typography.titleLarge)
            Text(text = "Matrícula: ${currentUser?.enrollment ?: "SXXXXXX"}", style = MaterialTheme.typography.bodyMedium)
            Text(text = "Correo: ${currentUser?.email ?: "Correo no disponible"}", style = MaterialTheme.typography.bodyMedium)

            Spacer(modifier = Modifier.height(16.dp))

            if (!isMyProfile) {
                Button(onClick = { /* TODO: Seguir usuario */ }) {
                    Text("Seguir")
                }
            }

            Spacer(modifier = Modifier.height(24.dp))

            Text("Publicaciones", style = MaterialTheme.typography.titleMedium)
            Spacer(modifier = Modifier.height(8.dp))

            if (loadError != null) {
                Text(text = loadError!!, color = MaterialTheme.colorScheme.error)
            } else {
                LazyVerticalGrid(
                    columns = GridCells.Fixed(3),
                    modifier = Modifier.fillMaxWidth().heightIn(min = 300.dp, max = 600.dp),
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
                                    .clickable { selectedPost = post }
                            )
                        }
                    }
                )
            }
        }

        selectedPost?.let { post ->
            var likeCount by remember { mutableStateOf(0) }
            var comments by remember { mutableStateOf<List<com.example.feigram.network.model.comments.Comment>>(emptyList()) }
            var newComment by remember { mutableStateOf("") }
            var isLiked by remember { mutableStateOf(false) }

            LaunchedEffect(post) {
                try {
                    val likeResponse = RetrofitInstance.postApi.getPostLikesCount(
                        postId = post.postId,
                        token = "Bearer ${currentUser?.token.orEmpty()}"
                    )
                    likeCount = likeResponse.likeCount

                    isLiked = RetrofitInstance.likeApi.checkLike(
                        userId = currentUser?.userId ?: "",
                        postId = post.postId.toString(),
                        token = "Bearer ${currentUser?.token.orEmpty()}"
                    )

                    val commentsResponse = RetrofitInstance.postApi.getPostComments(
                        postId = post.postId,
                        token = "Bearer ${currentUser?.token.orEmpty()}"
                    )

                    comments = commentsResponse.comments.map { c ->
                        val profile = try {
                            RetrofitInstance.profileApi.getProfileById(
                                id = c.userId,
                                token = "Bearer ${currentUser?.token.orEmpty()}"
                            )
                        } catch (e: Exception) {
                            null
                        }

                        com.example.feigram.network.model.comments.Comment(
                            username = profile?.name ?: c.userId,
                            profileImageUrl = profile?.photo ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                            text = c.textComment
                        )
                    }

                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }

            AlertDialog(
                onDismissRequest = { selectedPost = null },
                confirmButton = {},
                dismissButton = {
                    TextButton(onClick = { selectedPost = null }) {
                        Text("Cerrar")
                    }
                },
                text = {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(8.dp)
                            .heightIn(min = 600.dp, max = 800.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        AsyncImage(
                            model = post.urlMedia,
                            contentDescription = "Detalle publicación",
                            modifier = Modifier
                                .fillMaxWidth()
                                .aspectRatio(4f / 5f)
                                .clip(MaterialTheme.shapes.medium)
                        )

                        Divider(thickness = 0.5.dp, color = MaterialTheme.colorScheme.outlineVariant)
                        Spacer(modifier = Modifier.height(8.dp))

                        Text(post.description ?: "Sin descripción", style = MaterialTheme.typography.bodyMedium)
                        Spacer(modifier = Modifier.height(8.dp))

                        Divider(thickness = 0.5.dp, color = MaterialTheme.colorScheme.outlineVariant)
                        Spacer(modifier = Modifier.height(8.dp))

                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(16.dp),
                            modifier = Modifier.fillMaxWidth().padding(horizontal = 16.dp)
                        ) {
                            IconButton(onClick = {
                                scope.launch {
                                    try {
                                        if (isLiked) {
                                            RetrofitInstance.likeApi.deleteLike(
                                                like = com.example.feigram.network.model.likes.LikeRequest(
                                                    userId = currentUser?.userId ?: "",
                                                    postId = post.postId.toString()
                                                ),
                                                token = "Bearer ${currentUser?.token.orEmpty()}"
                                            )
                                            likeCount -= 1
                                            isLiked = false
                                        } else {
                                            RetrofitInstance.likeApi.addLike(
                                                like = com.example.feigram.network.model.likes.LikeRequest(
                                                    userId = currentUser?.userId ?: "",
                                                    postId = post.postId.toString()
                                                ),
                                                token = "Bearer ${currentUser?.token.orEmpty()}"
                                            )
                                            likeCount += 1
                                            isLiked = true
                                        }
                                    } catch (e: Exception) {
                                        e.printStackTrace()
                                    }
                                }
                            }) {
                                Icon(
                                    imageVector = Icons.Filled.Favorite,
                                    contentDescription = "Like",
                                    tint = if (isLiked) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.onSurface
                                )
                            }
                            Text("$likeCount Likes", style = MaterialTheme.typography.bodyMedium)
                            Text("${comments.size} Comentarios", style = MaterialTheme.typography.bodyMedium)
                        }

                        Spacer(modifier = Modifier.height(8.dp))

                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .weight(1f)
                                .verticalScroll(rememberScrollState())
                                .padding(8.dp)
                        ) {
                            comments.forEach { comment ->
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
                                            .size(36.dp)
                                            .clip(CircleShape)
                                            .padding(end = 8.dp)
                                    )
                                    Column {
                                        Text(comment.username, style = MaterialTheme.typography.labelMedium)
                                        Text(comment.text, style = MaterialTheme.typography.bodySmall)
                                    }
                                }
                                Divider(thickness = 0.5.dp, color = MaterialTheme.colorScheme.outlineVariant)
                            }
                        }

                        Spacer(modifier = Modifier.height(8.dp))

                        Row(
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            OutlinedTextField(
                                value = newComment,
                                onValueChange = { newComment = it },
                                label = { Text("Escribe un comentario...") },
                                modifier = Modifier.weight(1f)
                            )
                            IconButton(onClick = {
                                scope.launch {
                                    if (newComment.isNotBlank()) {
                                        try {
                                            RetrofitInstance.commentApi.addComment(
                                                comment = com.example.feigram.network.model.comments.CommentRequest(
                                                    userId = currentUser?.userId ?: "",
                                                    postId = post.postId.toString(),
                                                    textComment = newComment,
                                                    createdAt = java.time.Instant.now().toString()
                                                ),
                                                token = "Bearer ${currentUser?.token.orEmpty()}"
                                            )

                                            val updatedComments = RetrofitInstance.postApi.getPostComments(
                                                postId = post.postId,
                                                token = "Bearer ${currentUser?.token.orEmpty()}"
                                            )

                                            comments = updatedComments.comments.map { c ->
                                                val profile = try {
                                                    RetrofitInstance.profileApi.getProfileById(
                                                        id = c.userId,
                                                        token = "Bearer ${currentUser?.token.orEmpty()}"
                                                    )
                                                } catch (e: Exception) {
                                                    null
                                                }

                                                com.example.feigram.network.model.comments.Comment(
                                                    username = profile?.name ?: c.userId,
                                                    profileImageUrl = profile?.photo
                                                        ?: "https://randomuser.me/api/portraits/lego/1.jpg",
                                                    text = c.textComment
                                                )
                                            }

                                            newComment = ""

                                        } catch (e: Exception) {
                                            e.printStackTrace()
                                        }
                                    }
                                }
                            }) {
                                Icon(Icons.AutoMirrored.Filled.Send, contentDescription = "Enviar comentario")
                            }
                        }
                    }
                }
            )
        }

    }
}
