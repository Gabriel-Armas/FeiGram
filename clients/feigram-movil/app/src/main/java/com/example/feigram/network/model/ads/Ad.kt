package com.example.feigram.network.model.ads

data class Ad(
    val id: String,
    val brandName: String,
    val publicationDate: String,
    val urlMedia: String,
    val urlSite: String,
    val description: String,
    val amount: Int
)