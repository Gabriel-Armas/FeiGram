import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.TextFieldValue
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.feigram.network.model.ads.Ad
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.viewmodels.SessionViewModel
import kotlinx.coroutines.launch
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.asRequestBody
import okhttp3.RequestBody.Companion.toRequestBody
import androidx.compose.foundation.lazy.items
import java.io.File

@Composable
fun AdsScreen(
    navController: NavController,
    sessionViewModel: SessionViewModel,
) {
    val scope = rememberCoroutineScope()
    val context = LocalContext.current
    val authToken = sessionViewModel.userSession.collectAsState().value?.token ?: ""
    val adsApi = RetrofitInstance.adsApi

    var adsList by remember { mutableStateOf<List<Ad>>(emptyList()) }
    var searchQuery by remember { mutableStateOf(TextFieldValue("")) }
    var filteredAds by remember { mutableStateOf<List<Ad>>(emptyList()) }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    var showCreateDialog by remember { mutableStateOf(false) }
    var brandNameInput by remember { mutableStateOf("") }
    var urlSiteInput by remember { mutableStateOf("") }
    var descriptionInput by remember { mutableStateOf("") }
    var amountInput by remember { mutableStateOf("") }
    var selectedImageUri by remember { mutableStateOf<Uri?>(null) }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri: Uri? ->
        selectedImageUri = uri
    }

    LaunchedEffect(Unit) {
        isLoading = true
        try {
            val response = adsApi.getAllAds("Bearer $authToken")
            if (response.isSuccessful) {
                adsList = response.body()?.ads ?: emptyList()
                filteredAds = adsList
            } else {
                errorMessage = "Error al obtener anuncios: ${response.code()} ${response.message()}"
            }
        } catch (e: Exception) {
            errorMessage = "Error en la red: ${e.message}"
        }
        isLoading = false
    }

    fun searchAds(query: String) {
        filteredAds = if (query.isBlank()) {
            adsList
        } else {
            val lowerQuery = query.lowercase()
            adsList.filter {
                it.brandName.lowercase().contains(lowerQuery) ||
                        it.description.lowercase().contains(lowerQuery)
            }
        }
    }

    fun createAd() {
        if (brandNameInput.isBlank() || urlSiteInput.isBlank() || descriptionInput.isBlank() || amountInput.isBlank() || selectedImageUri == null) {
            errorMessage = "Por favor completa todos los campos y selecciona una imagen"
            return
        }
        scope.launch {
            try {
                val parcelFileDescriptor = context.contentResolver.openFileDescriptor(selectedImageUri!!, "r") ?: return@launch
                val inputStream = context.contentResolver.openInputStream(selectedImageUri!!) ?: return@launch
                val tempFile = File(context.cacheDir, "upload_image_temp.jpg")
                inputStream.use { input -> tempFile.outputStream().use { output -> input.copyTo(output) } }

                val requestFile = tempFile.asRequestBody("image/*".toMediaType())
                val multipartBody = MultipartBody.Part.createFormData("file", tempFile.name, requestFile)

                val brandNamePart = brandNameInput.toRequestBody("text/plain".toMediaType())
                val urlSitePart = urlSiteInput.toRequestBody("text/plain".toMediaType())
                val descriptionPart = descriptionInput.toRequestBody("text/plain".toMediaType())
                val amountPart = amountInput.toRequestBody("text/plain".toMediaType())

                isLoading = true
                val response = adsApi.createAd(
                    "Bearer $authToken",
                    brandNamePart,
                    urlSitePart,
                    descriptionPart,
                    amountPart,
                    multipartBody
                )
                isLoading = false

                if (response.isSuccessful) {
                    val refreshedResponse = adsApi.getAllAds("Bearer $authToken")
                    if (refreshedResponse.isSuccessful) {
                        adsList = refreshedResponse.body()?.ads ?: emptyList()
                        filteredAds = adsList
                        showCreateDialog = false
                        brandNameInput = ""
                        urlSiteInput = ""
                        descriptionInput = ""
                        amountInput = ""
                        selectedImageUri = null
                        errorMessage = null
                    } else {
                        errorMessage = "Error al recargar anuncios: ${refreshedResponse.code()} ${refreshedResponse.message()}"
                    }
                } else {
                    errorMessage = "Error al crear anuncio: ${response.code()} ${response.message()}"
                }
            } catch (e: Exception) {
                isLoading = false
                errorMessage = "Error en la red: ${e.message}"
            }
        }
    }

    Column(modifier = Modifier.fillMaxSize().statusBarsPadding().padding(16.dp)) {
        Text(text = "Gestión de Anuncios", style = MaterialTheme.typography.headlineMedium)

        Spacer(modifier = Modifier.height(16.dp))

        Button(onClick = { showCreateDialog = true }) {
            Text("Crear Nuevo Anuncio")
        }

        Spacer(modifier = Modifier.height(16.dp))

        BasicTextField(
            value = searchQuery,
            onValueChange = {
                searchQuery = it
                searchAds(it.text)
            },
            modifier = Modifier
                .fillMaxWidth()
                .padding(8.dp)
                .border(1.dp, MaterialTheme.colorScheme.primary, MaterialTheme.shapes.small)
                .padding(8.dp),
            singleLine = true,
            decorationBox = { innerTextField ->
                if (searchQuery.text.isEmpty()) {
                    Text(
                        "Buscar anuncios...",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                innerTextField()
            }
        )

        Spacer(modifier = Modifier.height(16.dp))

        if (isLoading) {
            CircularProgressIndicator()
        } else {
            errorMessage?.let { Text(text = it, color = MaterialTheme.colorScheme.error) }
            if (filteredAds.isEmpty()) {
                Text("No hay anuncios que mostrar", style = MaterialTheme.typography.bodyLarge)
            } else {
                LazyColumn(modifier = Modifier.fillMaxSize()) {
                    items(items = filteredAds, key = { ad -> ad.id }) { ad ->
                        AdItem(ad = ad, onDelete = {
                            scope.launch {
                                try {
                                    val deleteResponse = adsApi.deleteAd("Bearer $authToken", ad.id)
                                    if (deleteResponse.isSuccessful) {
                                        adsList = adsList.filter { it.id != ad.id }
                                        filteredAds = filteredAds.filter { it.id != ad.id }
                                    } else {
                                        errorMessage = "Error al eliminar anuncio: ${deleteResponse.code()}"
                                    }
                                } catch (e: Exception) {
                                    errorMessage = "Error en la red: ${e.message}"
                                }
                            }
                        })
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        AlertDialog(
            onDismissRequest = { showCreateDialog = false },
            title = { Text("Crear Anuncio") },
            text = {
                Column {
                    OutlinedTextField(
                        value = brandNameInput,
                        onValueChange = { brandNameInput = it },
                        label = { Text("Marca") },
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = urlSiteInput,
                        onValueChange = { urlSiteInput = it },
                        label = { Text("URL Sitio") },
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = descriptionInput,
                        onValueChange = { descriptionInput = it },
                        label = { Text("Descripción") },
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = amountInput,
                        onValueChange = { newValue ->
                            if (newValue.length <= 3 && newValue.all { it.isDigit() }) {
                                val number = newValue.toIntOrNull() ?: 0
                                if (number <= 100) {
                                    amountInput = newValue
                                }
                            } else if (newValue.isEmpty()) {
                                amountInput = ""
                            }
                        },
                        label = { Text("Cantidad pagada") },
                        singleLine = true,
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
                    )

                    Spacer(modifier = Modifier.height(8.dp))
                    Button(onClick = { launcher.launch("image/*") }) {
                        Text(if (selectedImageUri == null) "Seleccionar Imagen" else "Imagen Seleccionada")
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { createAd() }) {
                    Text("Crear")
                }
            },
            dismissButton = {
                TextButton(onClick = { showCreateDialog = false }) {
                    Text("Cancelar")
                }
            }
        )
    }
}

@Composable
fun AdItem(ad: Ad, onDelete: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 6.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Text(text = "Marca: ${ad.brandName}", style = MaterialTheme.typography.titleMedium)
            Text(text = "Descripción: ${ad.description}", style = MaterialTheme.typography.bodyMedium)
            Text(text = "Cantidad pagada: ${ad.amount}", style = MaterialTheme.typography.bodySmall)
            Text(text = "Fecha: ${ad.publicationDate}", style = MaterialTheme.typography.bodySmall)
            Spacer(modifier = Modifier.height(8.dp))
            Row(
                horizontalArrangement = Arrangement.End,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(
                    text = "Eliminar",
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier
                        .clickable { onDelete() }
                        .padding(8.dp)
                )
            }
        }
    }
}
