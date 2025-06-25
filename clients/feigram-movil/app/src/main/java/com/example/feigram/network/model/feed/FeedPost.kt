package com.example.feigram.network.model.feed

data class FeedPost(
    val post_id: Int,
    val id_usuario: String,
    val descripcion: String,
    val url_media: String,
    val fechaPublicacion: String,
    val comentarios: Int,
    val likes: Int
)