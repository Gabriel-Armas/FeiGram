package com.example.feigram.network.model.feed

import com.google.gson.annotations.SerializedName

data class PostRecommendation(
    @SerializedName("post_id")
    val postId: Int,
    @SerializedName("id_usuario")
    val userId: String,
    @SerializedName("descripcion")
    val description: String,
    @SerializedName("url_media")
    val mediaUrl: String,
    @SerializedName("fechaPublicacion")
    val publicationDate: String,
    @SerializedName("comentarios")
    val comments: Int,
    @SerializedName("likes")
    val likes: Int
)