package com.example.feigram.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.feigram.network.model.LoginRequest
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.service.SessionManager
import com.example.feigram.network.service.SessionManager.userSession
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class UserSession(
    val token: String,
    val userId: String,
    val username: String,
    val enrollment: String,
    val email: String,
    val profileImageUrl: String,
    val rol: String,
    val followerCount: Int
)

class SessionViewModel : ViewModel() {
    private val _userSession = MutableStateFlow<UserSession?>(null)
    val userSession: StateFlow<UserSession?> = _userSession

    fun login(email: String, password: String, onResult: (Boolean) -> Unit) {
        viewModelScope.launch {
            try {
                val authResponse = RetrofitInstance.authApi.login(LoginRequest(email, password))
                val token = authResponse.token
                val userId = authResponse.userId

                val profileResponse = RetrofitInstance.profileApi.getProfileById(
                    id = userId,
                    token = "Bearer $token"
                )

                _userSession.value = UserSession(
                    token = token,
                    userId = userId,
                    username = profileResponse.name,
                    enrollment = profileResponse.enrollment,
                    email = email,
                    profileImageUrl = profileResponse.photo ?: "",
                    rol = authResponse.rol,
                    followerCount = profileResponse.followerCount
                )

                SessionManager.userSession = _userSession.value

                onResult(true)
            } catch (e: Exception) {
                e.printStackTrace()
                onResult(false)
            }
        }
    }
}