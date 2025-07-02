package com.example.feigram.screens

import android.net.Uri
import android.os.FileUtils
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Block
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter
import com.example.feigram.network.model.Profile
import com.example.feigram.network.model.RegisterRequest
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.service.SessionManager.userSession
import com.example.feigram.screens.components.FileUtils.getFileFromUri
import com.example.feigram.viewmodels.SessionViewModel
import com.example.feigram.viewmodels.UserSession
import kotlinx.coroutines.launch
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.asRequestBody
import okhttp3.RequestBody.Companion.toRequestBody

data class ProfileWithRole(
    val profile: Profile,
    val email: String,
    val role: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AdminAccountsScreen(navController: NavController, sessionViewModel: SessionViewModel) {
    val currentUser by sessionViewModel.userSession.collectAsState()
    val scope = rememberCoroutineScope()

    var profilesWithRoles by remember { mutableStateOf<List<ProfileWithRole>>(emptyList()) }
    var isLoading by remember { mutableStateOf(true) }
    var loadError by remember { mutableStateOf<String?>(null) }
    var showAddDialog by remember { mutableStateOf(false) }

    fun loadProfiles() {
        scope.launch {
            isLoading = true
            try {
                val profiles = RetrofitInstance.profileApi.getAllProfiles(
                    token = "Bearer ${currentUser?.token.orEmpty()}"
                )

                val combinedProfiles = profiles.mapNotNull { profile ->
                    try {
                        val roleResponse = RetrofitInstance.authApi.getUserRole(
                            userId = profile.id,
                            token = "Bearer ${currentUser?.token.orEmpty()}"
                        )
                        ProfileWithRole(
                            profile = profile,
                            email = roleResponse.email,
                            role = roleResponse.role
                        )
                    } catch (e: Exception) {
                        e.printStackTrace()
                        null
                    }
                }

                profilesWithRoles = combinedProfiles
                loadError = null
            } catch (e: Exception) {
                e.printStackTrace()
                loadError = "Error al cargar cuentas"
            } finally {
                isLoading = false
            }
        }
    }

    LaunchedEffect(Unit) {
        loadProfiles()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Gestión de Cuentas", fontSize = 20.sp, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface
                ),
                actions = {
                    IconButton(onClick = { showAddDialog = true }) {
                        Icon(Icons.Default.Add, contentDescription = "Agregar cuenta")
                    }
                },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        }
    ) { padding ->
        Box(modifier = Modifier.fillMaxSize().padding(padding)) {
            when {
                isLoading -> CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                loadError != null -> Text(loadError!!, color = MaterialTheme.colorScheme.error)
                else -> {
                    LazyColumn(
                        modifier = Modifier.fillMaxSize(),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        items(profilesWithRoles) { profileWithRole ->
                            AccountItem(profileWithRole, currentUser) {
                                loadProfiles()
                            }
                        }
                    }
                }
            }
        }
    }

    if (showAddDialog) {
        AddAccountDialog(
            token = currentUser?.token.orEmpty(),
            onDismiss = { showAddDialog = false; loadProfiles() }
        )
    }
}

@Composable
fun AccountItem(
    profileWithRole: ProfileWithRole,
    currentUser: UserSession?,
    onActionCompleted: () -> Unit
) {
    val profile = profileWithRole.profile
    val isBanned = profileWithRole.role.equals("BANNED", ignoreCase = true)
    val scope = rememberCoroutineScope()

    var showEditDialog by remember { mutableStateOf(false) }

    Row(
        modifier = Modifier
            .fillMaxWidth()
            .background(
                if (isBanned) MaterialTheme.colorScheme.error.copy(alpha = 0.2f)
                else Color.Transparent
            )
            .padding(8.dp),
        verticalAlignment = Alignment.CenterVertically
    ){
        Image(
            painter = rememberAsyncImagePainter(profile.photo ?: "https://randomuser.me/api/portraits/lego/1.jpg"),
            contentDescription = "Foto de ${profile.name}",
            contentScale = ContentScale.Crop,
            modifier = Modifier
                .size(50.dp)
                .clip(CircleShape)
        )

        Spacer(modifier = Modifier.width(12.dp))

        Column(modifier = Modifier.weight(1f)) {
            Text(text = profile.name, fontSize = 16.sp, fontWeight = FontWeight.Medium)
            Text(text = "@${profile.enrollment}", fontSize = 13.sp, color = Color.Gray)
            Text(text = "Rol: ${profileWithRole.role}", fontSize = 12.sp, color = Color.DarkGray)
        }

        IconButton(onClick = {
            scope.launch {
                try {
                    val token = "Bearer ${currentUser?.token.orEmpty()}"
                    val body = mapOf("email" to profileWithRole.email)

                    if (isBanned) {
                        RetrofitInstance.authApi.unbanUser(body, token)
                    } else {
                        RetrofitInstance.authApi.banUser(body, token)
                    }
                    onActionCompleted()
                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }
        }) {
            Icon(
                imageVector = Icons.Default.Block,
                contentDescription = if (isBanned) "Desbanear" else "Banear",
                tint = if (isBanned) Color.Red else MaterialTheme.colorScheme.primary
            )
        }

        IconButton(onClick = { showEditDialog = true }) {
            Icon(Icons.Default.Edit, contentDescription = "Editar cuenta")
        }

        if (showEditDialog) {
            EditAccountDialog(
                profile = profileWithRole,
                token = currentUser?.token.orEmpty(),
                onDismiss = { showEditDialog = false },
                onProfileUpdated = onActionCompleted
            )
        }

    }
}

@Composable
fun AddAccountDialog(token: String, onDismiss: () -> Unit) {
    val scope = rememberCoroutineScope()
    val context = LocalContext.current

    var name by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var enrollment by remember { mutableStateOf("") }
    var sex by remember { mutableStateOf("") }
    var photoUri by remember { mutableStateOf<Uri?>(null) }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri ->
        photoUri = uri
    }

    AlertDialog(
        onDismissRequest = { onDismiss() },
        confirmButton = {
            TextButton(onClick = {
                scope.launch {
                    isLoading = true
                    try {
                        val namePart = name.toRequestBody("text/plain".toMediaTypeOrNull())
                        val emailPart = email.toRequestBody("text/plain".toMediaTypeOrNull())
                        val passwordPart = password.toRequestBody("text/plain".toMediaTypeOrNull())
                        val enrollmentPart = enrollment.toRequestBody("text/plain".toMediaTypeOrNull())
                        val sexPart = sex.toRequestBody("text/plain".toMediaTypeOrNull())

                        val photoPart = photoUri?.let { uri ->
                            val file = getFileFromUri(context, uri)  // aquí uso context
                            val requestFile = file.asRequestBody("image/*".toMediaTypeOrNull())
                            MultipartBody.Part.createFormData("photo", file.name, requestFile)
                        }

                        RetrofitInstance.authApi.register(
                            name = namePart,
                            email = emailPart,
                            password = passwordPart,
                            enrollment = enrollmentPart,
                            sex = sexPart,
                            photo = photoPart,
                            token = "Bearer ${userSession?.token.orEmpty()}"
                        )
                        onDismiss()
                    } catch (e: Exception) {
                        e.printStackTrace()
                        errorMessage = "Error al crear la cuenta"
                    } finally {
                        isLoading = false
                    }
                }
            }) {
                Text("Crear")
            }
        },
        dismissButton = {
            TextButton(onClick = { onDismiss() }) {
                Text("Cancelar")
            }
        },
        title = { Text("Nueva cuenta") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(value = name, onValueChange = { name = it }, label = { Text("Nombre") })
                OutlinedTextField(value = email, onValueChange = { email = it }, label = { Text("Email") })
                OutlinedTextField(value = password, onValueChange = { password = it }, label = { Text("Contraseña") })
                OutlinedTextField(value = enrollment, onValueChange = { enrollment = it }, label = { Text("Matrícula") })
                OutlinedTextField(value = sex, onValueChange = { sex = it }, label = { Text("Sexo") })

                Spacer(modifier = Modifier.height(8.dp))

                Button(onClick = { launcher.launch("image/*") }) {
                    Text("Seleccionar Foto")
                }
                photoUri?.let { Text("Foto seleccionada", color = MaterialTheme.colorScheme.primary) }

                if (errorMessage != null) {
                    Text(errorMessage!!, color = MaterialTheme.colorScheme.error)
                }
                if (isLoading) {
                    CircularProgressIndicator(modifier = Modifier.size(24.dp))
                }
            }
        }
    )
}

@Composable
fun EditAccountDialog(
    profile: ProfileWithRole,
    token: String,
    onDismiss: () -> Unit,
    onProfileUpdated: () -> Unit
) {
    val scope = rememberCoroutineScope()
    val context = LocalContext.current

    var name by remember { mutableStateOf(profile.profile.name) }
    var email by remember { mutableStateOf(profile.email) }
    var enrollment by remember { mutableStateOf(profile.profile.enrollment) }
    var sex by remember { mutableStateOf(profile.profile.sex) }
    var photoUri by remember { mutableStateOf<Uri?>(null) }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri ->
        photoUri = uri
    }

    AlertDialog(
        onDismissRequest = { onDismiss() },
        confirmButton = {
            TextButton(onClick = {
                scope.launch {
                    isLoading = true
                    try {
                        val namePart = name.toRequestBody("text/plain".toMediaTypeOrNull())
                        val sexPart = (sex?.ifBlank { "" })?.toRequestBody("text/plain".toMediaTypeOrNull())
                        val enrollmentPart = enrollment.toRequestBody("text/plain".toMediaTypeOrNull())

                        val photoPart = photoUri?.let { uri ->
                            val file = getFileFromUri(context, uri)
                            val requestFile = file.asRequestBody("image/*".toMediaTypeOrNull())
                            MultipartBody.Part.createFormData("photo", file.name, requestFile)
                        }

                        RetrofitInstance.profileApi.updateProfile(
                            id = profile.profile.id,
                            token = "Bearer $token",
                            name = namePart,
                            sex = sexPart,
                            enrollment = enrollmentPart,
                            photo = photoPart
                        )

                        if (email != profile.email) {
                            val body = mapOf("newEmail" to email)
                            RetrofitInstance.authApi.updateUserEmail(
                                userId = profile.profile.id,
                                request = body,
                                token = "Bearer $token"
                            )
                        }

                        onProfileUpdated()
                        onDismiss()
                    } catch (e: Exception) {
                        e.printStackTrace()
                        errorMessage = "Error al actualizar la cuenta"
                    } finally {
                        isLoading = false
                    }
                }
            }) {
                Text("Guardar")
            }
        },
        dismissButton = {
            TextButton(onClick = { onDismiss() }) {
                Text("Cancelar")
            }
        },
        title = { Text("Editar cuenta") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(value = name, onValueChange = { name = it }, label = { Text("Nombre") })
                OutlinedTextField(value = email, onValueChange = { email = it }, label = { Text("Email") })
                OutlinedTextField(value = enrollment, onValueChange = { enrollment = it }, label = { Text("Matrícula") })
                OutlinedTextField(value = sex!!, onValueChange = { sex = it }, label = { Text("Sexo") })

                Spacer(modifier = Modifier.height(8.dp))

                Button(onClick = { launcher.launch("image/*") }) {
                    Text("Seleccionar Foto")
                }
                photoUri?.let { Text("Foto seleccionada", color = MaterialTheme.colorScheme.primary) }

                if (errorMessage != null) {
                    Text(errorMessage!!, color = MaterialTheme.colorScheme.error)
                }
                if (isLoading) {
                    CircularProgressIndicator(modifier = Modifier.size(24.dp))
                }
            }
        }
    )
}