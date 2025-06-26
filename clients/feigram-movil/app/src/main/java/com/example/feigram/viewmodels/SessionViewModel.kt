package com.example.feigram.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.feigram.network.model.LoginRequest
import com.example.feigram.network.model.Profile
import com.example.feigram.network.service.RetrofitInstance
import com.example.feigram.network.service.SessionManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import okhttp3.MultipartBody

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

    fun refreshProfileData() {
        viewModelScope.launch {
            val currentToken = userSession.value?.token.orEmpty()
            val userId = userSession.value?.userId.orEmpty()

            if (userId.isNotBlank() && currentToken.isNotBlank()) {
                try {
                    val profile = RetrofitInstance.profileApi.getProfileById(
                        id = userId,
                        token = "Bearer $currentToken"
                    )

                    _userSession.update { oldSession ->
                        oldSession?.copy(
                            username = profile.name,
                            enrollment = profile.enrollment,
                            profileImageUrl = profile.photo.orEmpty(),
                            followerCount = profile.followerCount
                        )
                    }

                    SessionManager.userSession = _userSession.value

                } catch (e: Exception) {
                    e.printStackTrace()
                }
            }
        }
    }

    fun updateSessionProfile(profile: Profile) {
        _userSession.update { oldSession ->
            oldSession?.copy(
                username = profile.name,
                enrollment = profile.enrollment,
                profileImageUrl = profile.photo.orEmpty(),
                followerCount = profile.followerCount
            )
        }

        SessionManager.userSession = _userSession.value
    }

    fun updateProfilePhoto(profileId: String, photoPart: MultipartBody.Part, onError: (Throwable) -> Unit) {
        viewModelScope.launch {
            val currentToken = userSession.value?.token.orEmpty()
            if (currentToken.isBlank()) {
                onError(Throwable("Token no disponible"))
                return@launch
            }

            try {
                // 1. Subir la nueva foto
                RetrofitInstance.profileApi.updateProfile(
                    id = profileId,
                    token = "Bearer $currentToken",
                    name = null,
                    sex = null,
                    enrollment = null,
                    photo = photoPart
                )

                val updatedProfile = RetrofitInstance.profileApi.getProfileById(
                    id = profileId,
                    token = "Bearer $currentToken"
                )

                if (userSession.value?.userId == profileId) {
                    _userSession.update { oldSession ->
                        oldSession?.copy(
                            username = updatedProfile.name,
                            enrollment = updatedProfile.enrollment,
                            profileImageUrl = updatedProfile.photo.orEmpty(),
                            followerCount = updatedProfile.followerCount
                        )
                    }
                    SessionManager.userSession = _userSession.value
                }

            } catch (e: Exception) {
                onError(e)
            }
        }
    }

}
