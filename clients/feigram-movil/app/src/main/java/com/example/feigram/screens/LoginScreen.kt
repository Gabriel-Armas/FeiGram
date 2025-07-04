package com.example.feigram.screens

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavHostController
import com.example.feigram.R
import com.example.feigram.network.service.SessionManager.userSession
import com.example.feigram.network.service.WebSocketManager
import com.example.feigram.viewmodels.SessionViewModel

@Composable
fun LoginScreen(
    navController: NavHostController,
    sessionViewModel: SessionViewModel
) {
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var errorMessage by remember { mutableStateOf<String?>(null) }
    var isLoading by remember { mutableStateOf(false) }

    val coroutineScope = rememberCoroutineScope()

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(32.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Image(
                painter = painterResource(id = R.drawable.feigram_logo),
                contentDescription = "Logo",
                modifier = Modifier
                    .height(192.dp)
                    .padding(bottom = 32.dp)
            )

            Image(
                painter = painterResource(id = R.drawable.feigram_logofont),
                contentDescription = "LogoFont",
                modifier = Modifier
                    .height(128.dp)
                    .padding(bottom = 32.dp)
            )

            OutlinedTextField(
                value = email,
                onValueChange = { email = it },
                placeholder = { Text("Correo electrónico") },
                label = { Text("Correo electrónico") },
                modifier = Modifier.fillMaxWidth(),
                keyboardOptions = KeyboardOptions.Default.copy(keyboardType = KeyboardType.Email),
                isError = errorMessage?.contains("correo") == true
            )

            Spacer(modifier = Modifier.height(16.dp))

            OutlinedTextField(
                value = password,
                onValueChange = { password = it },
                placeholder = { Text("Contraseña") },
                label = { Text("Contraseña") },
                modifier = Modifier.fillMaxWidth(),
                visualTransformation = PasswordVisualTransformation(),
                keyboardOptions = KeyboardOptions.Default.copy(keyboardType = KeyboardType.Password),
                isError = errorMessage?.contains("contraseña") == true
            )

            if (errorMessage != null) {
                Spacer(modifier = Modifier.height(12.dp))
                Text(text = errorMessage!!, color = Color.Red)
            }

            Spacer(modifier = Modifier.height(24.dp))

            Button(
                onClick = {
                    errorMessage = null

                    if (email.isBlank()) {
                        errorMessage = "El correo electrónico no puede estar vacío"
                        return@Button
                    }

                    if (password.length < 4) {
                        errorMessage = "La contraseña debe tener al menos 4 caracteres"
                        return@Button
                    }

                    isLoading = true
                    sessionViewModel.login(email, password) { success ->
                        isLoading = false
                        if (success) {
                            navController.navigate("home") {
                                popUpTo("login") { inclusive = true }
                            }
                        } else {
                            errorMessage = "Credenciales inválidas o error de red"
                        }
                    }
                },
                modifier = Modifier.fillMaxWidth(),
                shape = MaterialTheme.shapes.medium,
                enabled = !isLoading
            ) {
                Text(if (isLoading) "Cargando..." else "Iniciar sesión")
            }

            Spacer(modifier = Modifier.height(16.dp))

            Text(
                //"¿Olvidaste tu contraseña?",
                "",
                color = Color.Blue,
                fontSize = 14.sp
            )
        }
    }
}
