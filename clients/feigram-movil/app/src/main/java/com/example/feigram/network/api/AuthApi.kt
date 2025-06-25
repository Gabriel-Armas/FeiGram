package com.example.feigram.network.api

import com.example.feigram.network.model.LoginRequest
import com.example.feigram.network.model.LoginResponse
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApi {
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): LoginResponse

    //Forgot password
}
