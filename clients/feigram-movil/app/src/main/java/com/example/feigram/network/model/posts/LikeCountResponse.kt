package com.example.feigram.network.model.posts

data class LikeCountResponse(
    val post_id: String,
    val like_count: Int
)