package com.example.feigram.network.api

import com.example.feigram.network.model.LoginRequest
import com.example.feigram.network.model.LoginResponse
import com.example.feigram.network.model.RegisterRequest
import com.example.feigram.network.model.UserRoleResponse
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Multipart
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Part
import retrofit2.http.Path

interface AuthApi {
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): LoginResponse

    @Multipart
    @POST("auth/register")
    suspend fun register(
        @Part("username") name: RequestBody,
        @Part("email") email: RequestBody,
        @Part("password") password: RequestBody,
        @Part("enrollment") enrollment: RequestBody,
        @Part("sex") sex: RequestBody,
        @Part photo: MultipartBody.Part?,
        @Header("Authorization") token: String
    )

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

    @PUT("auth/users/{id}/email")
    suspend fun updateUserEmail(
        @Path("id") userId: String,
        @Body request: Map<String, String>,
        @Header("Authorization") token: String
    )
}
