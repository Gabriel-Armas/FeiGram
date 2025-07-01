package com.example.feigram.screens

import android.content.pm.PackageManager
import android.net.Uri
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Camera
import androidx.compose.material.icons.filled.CameraAlt
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Photo
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import androidx.core.content.FileProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter
import com.example.feigram.network.model.posts.PostCreateRequest
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.service.SessionManager
import com.example.feigram.viewmodels.SessionViewModel
import kotlinx.coroutines.launch
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.asRequestBody
import java.io.File
import java.time.Instant

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NewPostScreen(
    navController: NavController,
    sessionViewModel: SessionViewModel = viewModel()
) {
    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()
    val userSession = SessionManager.userSession

    var imageUri by remember { mutableStateOf<Uri?>(null) }
    var description by remember { mutableStateOf("") }
    var galleryOpened by remember { mutableStateOf(false) }
    var showCameraPermissionDialog by remember { mutableStateOf(false) }
    var isUploading by remember { mutableStateOf(false) }

    val cameraImageUri = remember {
        val file = File.createTempFile("captured", ".jpg", context.cacheDir).apply {
            deleteOnExit()
        }
        FileProvider.getUriForFile(
            context,
            "${context.packageName}.fileprovider",
            file
        )
    }

    val cameraLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.TakePicture()
    ) { success ->
        if (success) imageUri = cameraImageUri
    }

    val cameraPermissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            cameraLauncher.launch(cameraImageUri)
        } else {
            Toast.makeText(context, "Permiso de cámara denegado", Toast.LENGTH_SHORT).show()
        }
    }

    val galleryLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri: Uri? ->
        imageUri = uri
    }

    LaunchedEffect(Unit) {
        if (!galleryOpened) {
            galleryOpened = true
            galleryLauncher.launch("image/*")
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Nueva publicación") }
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

            if (imageUri == null) {
                Text(
                    "Selecciona una imagen",
                    style = MaterialTheme.typography.titleMedium,
                    modifier = Modifier.padding(bottom = 16.dp)
                )

                OutlinedButton(
                    onClick = {
                        if (ContextCompat.checkSelfPermission(
                                context,
                                android.Manifest.permission.CAMERA
                            ) == PackageManager.PERMISSION_GRANTED
                        ) {
                            cameraLauncher.launch(cameraImageUri)
                        } else {
                            cameraPermissionLauncher.launch(android.Manifest.permission.CAMERA)
                        }
                    },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 8.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.CameraAlt,
                        contentDescription = "Abrir cámara",
                        modifier = Modifier.size(20.dp)
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Abrir Cámara")
                }

                OutlinedButton(
                    onClick = {
                        galleryLauncher.launch("image/*")
                    },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 8.dp)
                ) {
                    Icon(
                        imageVector = Icons.Filled.Photo,
                        contentDescription = "Abrir galería",
                        modifier = Modifier.size(20.dp)
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Abrir Galería")
                }
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

                OutlinedTextField(
                    value = description,
                    onValueChange = { description = it },
                    label = { Text("Descripción") },
                    modifier = Modifier.fillMaxWidth()
                )

                Button(
                    onClick = {
                        isUploading = true
                        coroutineScope.launch {
                            try {
                                val inputStream =
                                    context.contentResolver.openInputStream(imageUri!!)
                                val tempFile =
                                    File.createTempFile("upload", ".jpg", context.cacheDir)
                                inputStream?.use { input ->
                                    tempFile.outputStream().use { output ->
                                        input.copyTo(output)
                                    }
                                }

                                val requestBody =
                                    tempFile.asRequestBody("image/*".toMediaTypeOrNull())
                                val multipart = MultipartBody.Part.createFormData(
                                    "file",
                                    tempFile.name,
                                    requestBody
                                )

                                val uploadResponse = RetrofitInstance.postApi.uploadImage(
                                    file = multipart,
                                    token = "Bearer ${userSession?.token.orEmpty()}"
                                )

                                val post = PostCreateRequest(
                                    description = description,
                                    urlMedia = uploadResponse.url,
                                    publicationDate = Instant.now().toString()
                                )

                                RetrofitInstance.postApi.createPost(
                                    post = post,
                                    token = "Bearer ${userSession?.token.orEmpty()}"
                                )

                                Toast.makeText(context, "Publicación creada", Toast.LENGTH_SHORT)
                                    .show()
                                navController.popBackStack()

                            } catch (e: Exception) {
                                e.printStackTrace()
                                Toast.makeText(context, "Error al publicar", Toast.LENGTH_SHORT)
                                    .show()
                            } finally {
                                isUploading = false
                            }
                        }
                    },
                    enabled = description.isNotBlank() && !isUploading
                ) {
                    if (isUploading) {
                        CircularProgressIndicator(
                            color = MaterialTheme.colorScheme.onPrimary,
                            strokeWidth = 2.dp,
                            modifier = Modifier.size(20.dp)
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Publicando...")
                    } else {
                        Text("Publicar")
                    }
                }

            }
        }
    }
}