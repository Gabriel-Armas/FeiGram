package com.example.feigram.screens.components

import android.content.Context
import android.net.Uri
import java.io.File
import java.io.FileOutputStream
import java.io.InputStream

object FileUtils {
    fun getFileFromUri(context: Context, uri: Uri): File {
        val inputStream: InputStream? = context.contentResolver.openInputStream(uri)
        val tempFile = File.createTempFile("upload", null, context.cacheDir)
        tempFile.outputStream().use { fileOut ->
            inputStream?.copyTo(fileOut)
        }
        return tempFile
    }
}
