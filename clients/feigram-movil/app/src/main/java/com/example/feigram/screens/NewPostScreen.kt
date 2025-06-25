package com.example.feigram.screens

import android.content.pm.PackageManager
import android.net.Uri
import android.widget.Toast
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
            Toast.makeText(context, "Se requiere permiso de cámara", Toast.LENGTH_SHORT).show()
        }
    }

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
                            Toast.makeText(context, "Click palomita", Toast.LENGTH_SHORT).show()

                            if (imageUri == null) {
                                Toast.makeText(context, "No hay imagen seleccionada", Toast.LENGTH_SHORT).show()
                                return@IconButton
                            }

                            if (description.isBlank()) {
                                Toast.makeText(context, "La descripción está vacía", Toast.LENGTH_SHORT).show()
                                return@IconButton
                            }

                            if (userSession == null) {
                                Toast.makeText(context, "No hay sesión de usuario", Toast.LENGTH_SHORT).show()
                                return@IconButton
                            }

                            coroutineScope.launch {
                                try {
                                    Toast.makeText(context, "Iniciando carga de imagen...", Toast.LENGTH_SHORT).show()

                                    val inputStream = context.contentResolver.openInputStream(imageUri!!)
                                    if (inputStream == null) {
                                        Toast.makeText(context, "No se pudo abrir InputStream", Toast.LENGTH_SHORT).show()
                                        return@launch
                                    }

                                    val tempFile = File.createTempFile("upload", ".jpg", context.cacheDir)
                                    inputStream.use { input ->
                                        tempFile.outputStream().use { output ->
                                            input.copyTo(output)
                                        }
                                    }

                                    Toast.makeText(context, "Imagen copiada a temp", Toast.LENGTH_SHORT).show()

                                    val requestBody = tempFile.asRequestBody("image/*".toMediaTypeOrNull())
                                    val multipart = MultipartBody.Part.createFormData("file", tempFile.name, requestBody)

                                    val uploadResponse = RetrofitInstance.postApi.uploadImage(
                                        file = multipart,
                                        token = "Bearer ${userSession.token}"
                                    )

                                    Toast.makeText(context, "Imagen subida: URL: ${uploadResponse.url}", Toast.LENGTH_LONG).show()

                                    val post = PostCreateRequest(
                                        description = description,
                                        urlMedia = uploadResponse.url,
                                        publicationDate = Instant.now().toString()
                                    )

                                    RetrofitInstance.postApi.createPost(
                                        post = post,
                                        token = "Bearer ${userSession.token}"
                                    )

                                    Toast.makeText(context, "Publicación creada", Toast.LENGTH_SHORT).show()
                                    navController.popBackStack()

                                } catch (e: Exception) {
                                    e.printStackTrace()
                                    Toast.makeText(context, "Error: ${e.localizedMessage}", Toast.LENGTH_LONG).show()
                                }
                            }
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
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(onClick = {
                    galleryLauncher.launch("image/*")
                }) {
                    Text("Galería")
                }

                Button(onClick = {
                    if (ContextCompat.checkSelfPermission(context, android.Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED) {
                        cameraLauncher.launch(cameraImageUri)
                    } else {
                        cameraPermissionLauncher.launch(android.Manifest.permission.CAMERA)
                    }
                }) {
                    Text("Cámara")
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
