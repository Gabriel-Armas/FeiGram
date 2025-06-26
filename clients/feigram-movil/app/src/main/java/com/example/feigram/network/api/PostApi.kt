package com.example.feigram.network.api

import com.example.feigram.network.model.posts.CommentListResponse
import com.example.feigram.network.model.posts.LikeCountResponse
import com.example.feigram.network.model.posts.MessageResponse
import com.example.feigram.network.model.posts.PostCreateRequest
import com.example.feigram.network.model.posts.PostCreateResponse
import com.example.feigram.network.model.posts.PostResponse
import com.example.feigram.network.model.posts.UploadImageResponse
import okhttp3.MultipartBody
import retrofit2.http.*

interface PostApi {

    @POST("/posts/posts")
    suspend fun createPost(
        @Body post: PostCreateRequest,
        @Header("Authorization") token: String
    ): PostCreateResponse

    @Multipart
    @POST("/posts/upload-image")
    suspend fun uploadImage(
        @Part file: MultipartBody.Part,
        @Header("Authorization") token: String
    ): UploadImageResponse

    @GET("/posts/posts/user/{id_usuario}")
    suspend fun getUserPosts(
        @Path("id_usuario") userId: String,
        @Header("Authorization") token: String
    ): List<PostResponse>

    @DELETE("/posts/posts/{post_id}")
    suspend fun deletePost(
        @Path("post_id") postId: Int,
        @Header("Authorization") token: String
    ): MessageResponse

    @GET("/posts/posts/{post_id}/comments")
    suspend fun getPostComments(
        @Path("post_id") postId: Int,
        @Header("Authorization") token: String
    ): CommentListResponse

    @GET("/posts/posts/{post_id}/likes-count")
    suspend fun getPostLikesCount(
        @Path("post_id") postId: Int,
        @Header("Authorization") token: String
    ): LikeCountResponse
}
