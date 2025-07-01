package com.example.feigram.network.api

import com.example.feigram.network.model.LoginRequest
import com.example.feigram.network.model.LoginResponse
import com.example.feigram.network.model.RegisterRequest
import com.example.feigram.network.model.UserRoleResponse
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.POST
import retrofit2.http.Path

interface AuthApi {
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): LoginResponse

    @POST("auth/register")
    suspend fun register(@Body request: RegisterRequest): Unit

    @GET("auth/users/{id}")
    suspend fun getUserRole(
        @Path("id") userId: String,
        @Header("Authorization") token: String
    ): UserRoleResponse

    @POST("auth/ban-user")
    suspend fun banUser(
        @Body request: Map<String, String>,
        @Header("Authorization") token: String
    )

    @POST("auth/unban-user")
    suspend fun unbanUser(
        @Body request: Map<String, String>,
        @Header("Authorization") token: String
    )
}
