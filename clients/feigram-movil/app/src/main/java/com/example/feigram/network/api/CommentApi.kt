package com.example.feigram.network.api

import com.example.feigram.network.model.comments.CommentRequest
import com.example.feigram.network.model.comments.CommentResponse
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.Header
import retrofit2.http.POST
import retrofit2.http.Path

interface CommentApi {

    @POST("/comments/comments")
    suspend fun addComment(
        @Body comment: CommentRequest,
        @Header("Authorization") token: String
    ): CommentResponse

    @DELETE("/comments/comments/{id}")
    suspend fun deleteComment(
        @Path("id") id: Int,
        @Header("Authorization") token: String
    ): Unit
}
