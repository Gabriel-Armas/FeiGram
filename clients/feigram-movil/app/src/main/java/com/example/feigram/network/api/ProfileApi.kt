package com.example.feigram.network.api

import com.example.feigram.network.model.Profile
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.http.*

interface ProfileApi {

    @GET("profiles/profiles/{id}")
    suspend fun getProfileById(
        @Path("id") id: String,
        @Header("Authorization") token: String
    ): Profile

    @GET("profiles/profiles/{id}/following")
    suspend fun getFollowingList(
        @Path("id") userId: String,
        @Header("Authorization") token: String
    ): List<Profile>

    @GET("profiles/profiles/enrollment/{enrollment}")
    suspend fun getProfileByEnrollment(
        @Path("enrollment") enrollment: String,
        @Header("Authorization") token: String
    ): Profile

    @GET("profiles/profiles/search/{name}")
    suspend fun searchProfilesByName(
        @Path("name") name: String,
        @Header("Authorization") token: String
    ): List<Profile>

    @Multipart
    @PUT("profiles/profiles/{id}")
    suspend fun updateProfile(
        @Path("id") id: String,
        @Header("Authorization") token: String,
        @Part("Name") name: RequestBody?,
        @Part("Sex") sex: RequestBody?,
        @Part("Enrollment") enrollment: RequestBody?,
        @Part photo: MultipartBody.Part?
    ): Unit
}
