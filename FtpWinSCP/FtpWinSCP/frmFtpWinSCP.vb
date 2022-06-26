﻿#Region "About"
' / --------------------------------------------------------------------------------
' / Developer : Mr.Surapon Yodsanga (Thongkorn Tubtimkrob)
' / eMail : thongkorn@hotmail.com
' / URL: http://www.g2gnet.com (Khon Kaen - Thailand)
' / Facebook: https://www.facebook.com/g2gnet (For Thailand)
' / Facebook: https://www.facebook.com/commonindy (Worldwide)
' / More Info: http://www.g2gsoft.com/webboard
' /
' / Purpose:  Upload image into Hosting with FTP and download file to show on PictureBox.
' /              Used WinSCP .NET.
' / Microsoft Visual Basic .NET (2010)
' /
' / This is open source code under @CopyLeft by Thongkorn Tubtimkrob.
' / You can modify and/or distribute without to inform the developer.
' / --------------------------------------------------------------------------------
#End Region

Imports System.IO
Imports System.Net
Imports WinSCP

Public Class frmFtpWinSCP
    ' / --------------------------------------------------------------------------------
    '// แยกชื่อไฟล์+นามสกุล เพื่อทำการ Upload ไปยัง Hosting
    Dim UploadFileName As String = ""
    '// Default path.
    Dim PicturePath As String = MyPath(Application.StartupPath) & "Images\"

    '// ตำแหน่งของไฟล์ภาพบน Hosting
    Dim MyURL As String = "http://DOMAINNAME.com/upload/"
    '// ตำแหน่งในการอัพโหลดไฟล์ภาพ (FTP)
    Dim RemoteDir As String = "DOMAINNAME.com/upload/"

    '// For Login to Hosting.
    Dim HostName As String = "DOMAINNAME.com"
    Dim UName As String = "USERNAME"
    Dim Pwd As String = "PASSWORD"

    ' / --------------------------------------------------------------------------------
    ' / START HERE
    Private Sub frmFtpWinSCP_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        txtLocalFileName.Text = ""
        '/ Local image file.
        picData.Image = Image.FromFile(PicturePath + "NoImage.gif")
        picURL.Image = Image.FromFile(PicturePath + "NoImage.gif")
        Label3.Text = MyURL
        Label1.Text = "File Upload: "
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / เลือกรูปภาพในการ Upload.
    Private Sub btnBrowse_Click(sender As System.Object, e As System.EventArgs) Handles btnBrowse.Click
        Dim dlgImage As OpenFileDialog = New OpenFileDialog()
        ' / Open File Dialog
        With dlgImage
            '.InitialDirectory = PicturePath
            .Title = "เลือกภาพ"
            .Filter = "รูปแบบภาพ (*.jpg;*.png;*.gif;*.bmp)|*.jpg;*.png;*.gif;*.bmp"
            .FilterIndex = 1
            .RestoreDirectory = True
        End With
        ' Select OK after Browse ...
        If dlgImage.ShowDialog() = DialogResult.OK Then
            ' Get file size
            Dim info As New FileInfo(dlgImage.FileName)
            If (info.Length / 1024) > 1024 Then
                MessageBox.Show("ไฟล์ภาพที่คุณเลือกมีขนาด " & Format((info.Length / 1024), "#,##0") & " KB. ซึ่งมีขนาดใหญ่เกินกว่า 1,024 KB.", "รายงานสถานะ", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If
            '//
            txtLocalFileName.Text = dlgImage.FileName
            '// Put the current image file into PictureBox Control
            picData.Image = Image.FromFile(dlgImage.FileName)
            '// เอาเฉพาะชื่อไฟล์และนามสกุลภาพ (Filename + Extension) เช่น thongkorn.png
            UploadFileName = dlgImage.SafeFileName
            Label1.Text = "File Upload: " & txtLocalFileName.Text
        End If
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / เคลียร์รูปภาพออกจาก PictureBox และ TextBox 
    Private Sub btnDeleteImg_Click(sender As System.Object, e As System.EventArgs) Handles btnDeleteImg.Click
        txtLocalFileName.Clear()
        '/ Local image file.
        picData.Image = Image.FromFile(PicturePath & "NoImage.gif")
        picURL.Image = Image.FromFile(PicturePath & "NoImage.gif")
        UploadFileName = ""
        Label1.Text = "File Upload: "
    End Sub

    ' / --------------------------------------------------------------------------------
    '/ UPLOAD FILE.
    Private Sub btnUpload_Click(sender As System.Object, e As System.EventArgs) Handles btnUpload.Click
        If txtLocalFileName.Text.Trim.Length = 0 Then Return
        If picData.Image Is Nothing Or UploadFileName Is Nothing Or UploadFileName.Length = 0 Or UploadFileName = "" Then
            MessageBox.Show("Please select the image file first.", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        '//
        Me.Cursor = Cursors.WaitCursor
        Try
            '/ Setup session options
            Dim sessionOptions As New SessionOptions
            With sessionOptions
                .Protocol = Protocol.Ftp
                .HostName = HostName
                .UserName = UName
                .Password = Pwd
                '/ FTP ไม่ต้องใช้
                '.SshHostKeyFingerprint = "ssh-rsa 2048 XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX"
            End With

            Using Session As New Session
                '/ Connect
                Session.Open(sessionOptions)
                '/ Upload files
                Dim TransferOptions As New TransferOptions
                TransferOptions.TransferMode = TransferMode.Binary

                Dim TransferResult As TransferOperationResult
                TransferResult = Session.PutFiles(txtLocalFileName.Text, RemoteDir, False, TransferOptions)
                '/ Throw on any error
                TransferResult.Check()

                '/ Show results.
                For Each Transfer In TransferResult.Transfers
                    MessageBox.Show("Upload " & Transfer.FileName & " Successful.")
                Next
            End Using

            '// Download image file and show on PictureBox (picURL).
            Dim Req As WebRequest = WebRequest.Create(MyURL & UploadFileName)
            Dim Res As WebResponse = Req.GetResponse()
            Dim imgStream As Stream = Res.GetResponseStream()
            Dim imgPic As Image = Image.FromStream(imgStream)
            imgStream.Close()
            '/ Load File Stream into PictureBox.
            With picURL
                picURL.Image = imgPic
                .WaitOnLoad = True
                .SizeMode = PictureBoxSizeMode.StretchImage
            End With

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        Me.Cursor = Cursors.Default
        Label1.Text = "File Upload: " & txtLocalFileName.Text
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Get my project path
    ' / AppPath = C:\My Project\bin\debug
    ' / Replace "\bin\debug" with "\"
    ' / Return : C:\My Project\
    Function MyPath(ByVal AppPath As String) As String
        '/ Return Value
        MyPath = AppPath.ToLower.Replace("\bin\debug", "\").Replace("\bin\release", "\").Replace("\bin\x86\debug", "\").Replace("\bin\x86\release", "\")
        '// If not found folder then put the \ (BackSlash) at the end.
        If Microsoft.VisualBasic.Right(MyPath, 1) <> Chr(92) Then MyPath = MyPath & Chr(92)
    End Function

    Private Sub txtLocalFileName_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtLocalFileName.KeyPress
        '/ Lock keypress.
        e.Handled = True
    End Sub

    Private Sub frmFtpWinSCP_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        Me.Dispose()
        GC.SuppressFinalize(Me)
        Application.Exit()
    End Sub

End Class
