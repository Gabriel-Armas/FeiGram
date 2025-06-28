package com.example.feigram.network.api

import com.example.feigram.network.model.likes.LikeRequest
import com.example.feigram.network.model.likes.LikeResponse
import retrofit2.http.*

interface LikeApi {

    @GET("/likes/likes/check")
    suspend fun checkLike(
        @Query("userId") userId: String,
        @Query("postId") postId: String,
        @Header("Authorization") token: String
    ): Boolean

    @POST("/likes/likes")
    suspend fun addLike(
        @Body like: LikeRequest,
        @Header("Authorization") token: String
    ): LikeResponse

    @HTTP(method = "DELETE", path = "/likes/likes", hasBody = true)
    suspend fun deleteLike(
        @Body like: LikeRequest,
        @Header("Authorization") token: String
    ): Unit
}
