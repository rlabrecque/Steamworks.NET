// This file is automatically generated!

using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	public static class SteamRemoteStorage {
			// NOTE
			//
			// Filenames are case-insensitive, and will be converted to lowercase automatically.
			// So "foo.bar" and "Foo.bar" are the same file, and if you write "Foo.bar" then
			// iterate the files, the filename returned will be "foo.bar".
			//
			// file operations
		public static bool FileWrite(string pchFile, byte[] pvData, int cubData) {
			return NativeMethods.ISteamRemoteStorage_FileWrite(new InteropHelp.UTF8String(pchFile), pvData, cubData);
		}

		public static int FileRead(string pchFile, byte[] pvData, int cubDataToRead) {
			return NativeMethods.ISteamRemoteStorage_FileRead(new InteropHelp.UTF8String(pchFile), pvData, cubDataToRead);
		}

		public static bool FileForget(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FileForget(new InteropHelp.UTF8String(pchFile));
		}

		public static bool FileDelete(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FileDelete(new InteropHelp.UTF8String(pchFile));
		}

		public static SteamAPICall_t FileShare(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FileShare(new InteropHelp.UTF8String(pchFile));
		}

		public static bool SetSyncPlatforms(string pchFile, ERemoteStoragePlatform eRemoteStoragePlatform) {
			return NativeMethods.ISteamRemoteStorage_SetSyncPlatforms(new InteropHelp.UTF8String(pchFile), eRemoteStoragePlatform);
		}

			// file operations that cause network IO
		public static UGCFileWriteStreamHandle_t FileWriteStreamOpen(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FileWriteStreamOpen(new InteropHelp.UTF8String(pchFile));
		}

		public static bool FileWriteStreamWriteChunk(UGCFileWriteStreamHandle_t writeHandle, byte[] pvData, int cubData) {
			return NativeMethods.ISteamRemoteStorage_FileWriteStreamWriteChunk(writeHandle, pvData, cubData);
		}

		public static bool FileWriteStreamClose(UGCFileWriteStreamHandle_t writeHandle) {
			return NativeMethods.ISteamRemoteStorage_FileWriteStreamClose(writeHandle);
		}

		public static bool FileWriteStreamCancel(UGCFileWriteStreamHandle_t writeHandle) {
			return NativeMethods.ISteamRemoteStorage_FileWriteStreamCancel(writeHandle);
		}

			// file information
		public static bool FileExists(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FileExists(new InteropHelp.UTF8String(pchFile));
		}

		public static bool FilePersisted(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FilePersisted(new InteropHelp.UTF8String(pchFile));
		}

		public static int GetFileSize(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_GetFileSize(new InteropHelp.UTF8String(pchFile));
		}

		public static long GetFileTimestamp(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_GetFileTimestamp(new InteropHelp.UTF8String(pchFile));
		}

		public static ERemoteStoragePlatform GetSyncPlatforms(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_GetSyncPlatforms(new InteropHelp.UTF8String(pchFile));
		}

			// iteration
		public static int GetFileCount() {
			return NativeMethods.ISteamRemoteStorage_GetFileCount();
		}

		public static string GetFileNameAndSize(int iFile, out int pnFileSizeInBytes) {
			return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamRemoteStorage_GetFileNameAndSize(iFile, out pnFileSizeInBytes));
		}

			// configuration management
		public static bool GetQuota(out int pnTotalBytes, out int puAvailableBytes) {
			return NativeMethods.ISteamRemoteStorage_GetQuota(out pnTotalBytes, out puAvailableBytes);
		}

		public static bool IsCloudEnabledForAccount() {
			return NativeMethods.ISteamRemoteStorage_IsCloudEnabledForAccount();
		}

		public static bool IsCloudEnabledForApp() {
			return NativeMethods.ISteamRemoteStorage_IsCloudEnabledForApp();
		}

		public static void SetCloudEnabledForApp(bool bEnabled) {
			NativeMethods.ISteamRemoteStorage_SetCloudEnabledForApp(bEnabled);
		}

			// user generated content
			// Downloads a UGC file.  A priority value of 0 will download the file immediately,
			// otherwise it will wait to download the file until all downloads with a lower priority
			// value are completed.  Downloads with equal priority will occur simultaneously.
		public static SteamAPICall_t UGCDownload(UGCHandle_t hContent, uint unPriority) {
			return NativeMethods.ISteamRemoteStorage_UGCDownload(hContent, unPriority);
		}

			// Gets the amount of data downloaded so far for a piece of content. pnBytesExpected can be 0 if function returns false
			// or if the transfer hasn't started yet, so be careful to check for that before dividing to get a percentage
		public static bool GetUGCDownloadProgress(UGCHandle_t hContent, out int pnBytesDownloaded, out int pnBytesExpected) {
			return NativeMethods.ISteamRemoteStorage_GetUGCDownloadProgress(hContent, out pnBytesDownloaded, out pnBytesExpected);
		}

			// Gets metadata for a file after it has been downloaded. This is the same metadata given in the RemoteStorageDownloadUGCResult_t call result
		public static bool GetUGCDetails(UGCHandle_t hContent, out AppId_t pnAppID, out string ppchName, out int pnFileSizeInBytes, out CSteamID pSteamIDOwner) {
			IntPtr ppchName2;
			bool ret = NativeMethods.ISteamRemoteStorage_GetUGCDetails(hContent, out pnAppID, out ppchName2, out pnFileSizeInBytes, out pSteamIDOwner);
			ppchName = ret ? InteropHelp.PtrToStringUTF8(ppchName2) : null;
			return ret;
		}

			// After download, gets the content of the file.
			// Small files can be read all at once by calling this function with an offset of 0 and cubDataToRead equal to the size of the file.
			// Larger files can be read in chunks to reduce memory usage (since both sides of the IPC client and the game itself must allocate
			// enough memory for each chunk).  Once the last byte is read, the file is implicitly closed and further calls to UGCRead will fail
			// unless UGCDownload is called again.
			// For especially large files (anything over 100MB) it is a requirement that the file is read in chunks.
		public static int UGCRead(UGCHandle_t hContent, byte[] pvData, int cubDataToRead, uint cOffset, EUGCReadAction eAction) {
			return NativeMethods.ISteamRemoteStorage_UGCRead(hContent, pvData, cubDataToRead, cOffset, eAction);
		}

			// Functions to iterate through UGC that has finished downloading but has not yet been read via UGCRead()
		public static int GetCachedUGCCount() {
			return NativeMethods.ISteamRemoteStorage_GetCachedUGCCount();
		}

		public static UGCHandle_t GetCachedUGCHandle(int iCachedContent) {
			return NativeMethods.ISteamRemoteStorage_GetCachedUGCHandle(iCachedContent);
		}

			// The following functions are only necessary on the Playstation 3. On PC & Mac, the Steam client will handle these operations for you
			// On Playstation 3, the game controls which files are stored in the cloud, via FilePersist, FileFetch, and FileForget.
#if _PS3 || _SERVER
			// Connect to Steam and get a list of files in the Cloud - results in a RemoteStorageAppSyncStatusCheck_t callback
		public static void GetFileListFromServer() {
			NativeMethods.ISteamRemoteStorage_GetFileListFromServer();
		}

			// Indicate this file should be downloaded in the next sync
		public static bool FileFetch(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FileFetch(new InteropHelp.UTF8String(pchFile));
		}

			// Indicate this file should be persisted in the next sync
		public static bool FilePersist(string pchFile) {
			return NativeMethods.ISteamRemoteStorage_FilePersist(new InteropHelp.UTF8String(pchFile));
		}

			// Pull any requested files down from the Cloud - results in a RemoteStorageAppSyncedClient_t callback
		public static bool SynchronizeToClient() {
			return NativeMethods.ISteamRemoteStorage_SynchronizeToClient();
		}

			// Upload any requested files to the Cloud - results in a RemoteStorageAppSyncedServer_t callback
		public static bool SynchronizeToServer() {
			return NativeMethods.ISteamRemoteStorage_SynchronizeToServer();
		}

			// Reset any fetch/persist/etc requests
		public static bool ResetFileRequestState() {
			return NativeMethods.ISteamRemoteStorage_ResetFileRequestState();
		}
#endif
			// publishing UGC
		public static SteamAPICall_t PublishWorkshopFile(string pchFile, string pchPreviewFile, AppId_t nConsumerAppId, string pchTitle, string pchDescription, ERemoteStoragePublishedFileVisibility eVisibility, System.Collections.Generic.IList<string> pTags, EWorkshopFileType eWorkshopFileType) {
			return NativeMethods.ISteamRemoteStorage_PublishWorkshopFile(new InteropHelp.UTF8String(pchFile), new InteropHelp.UTF8String(pchPreviewFile), nConsumerAppId, new InteropHelp.UTF8String(pchTitle), new InteropHelp.UTF8String(pchDescription), eVisibility, new InteropHelp.SteamParamStringArray(pTags), eWorkshopFileType);
		}

		public static PublishedFileUpdateHandle_t CreatePublishedFileUpdateRequest(PublishedFileId_t unPublishedFileId) {
			return NativeMethods.ISteamRemoteStorage_CreatePublishedFileUpdateRequest(unPublishedFileId);
		}

		public static bool UpdatePublishedFileFile(PublishedFileUpdateHandle_t updateHandle, string pchFile) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFileFile(updateHandle, new InteropHelp.UTF8String(pchFile));
		}

		public static bool UpdatePublishedFilePreviewFile(PublishedFileUpdateHandle_t updateHandle, string pchPreviewFile) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFilePreviewFile(updateHandle, new InteropHelp.UTF8String(pchPreviewFile));
		}

		public static bool UpdatePublishedFileTitle(PublishedFileUpdateHandle_t updateHandle, string pchTitle) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFileTitle(updateHandle, new InteropHelp.UTF8String(pchTitle));
		}

		public static bool UpdatePublishedFileDescription(PublishedFileUpdateHandle_t updateHandle, string pchDescription) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFileDescription(updateHandle, new InteropHelp.UTF8String(pchDescription));
		}

		public static bool UpdatePublishedFileVisibility(PublishedFileUpdateHandle_t updateHandle, ERemoteStoragePublishedFileVisibility eVisibility) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFileVisibility(updateHandle, eVisibility);
		}

		public static bool UpdatePublishedFileTags(PublishedFileUpdateHandle_t updateHandle, System.Collections.Generic.IList<string> pTags) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFileTags(updateHandle, new InteropHelp.SteamParamStringArray(pTags));
		}

		public static SteamAPICall_t CommitPublishedFileUpdate(PublishedFileUpdateHandle_t updateHandle) {
			return NativeMethods.ISteamRemoteStorage_CommitPublishedFileUpdate(updateHandle);
		}

			// Gets published file details for the given publishedfileid.  If unMaxSecondsOld is greater than 0,
			// cached data may be returned, depending on how long ago it was cached.  A value of 0 will force a refresh.
			// A value of k_WorkshopForceLoadPublishedFileDetailsFromCache will use cached data if it exists, no matter how old it is.
		public static SteamAPICall_t GetPublishedFileDetails(PublishedFileId_t unPublishedFileId, uint unMaxSecondsOld) {
			return NativeMethods.ISteamRemoteStorage_GetPublishedFileDetails(unPublishedFileId, unMaxSecondsOld);
		}

		public static SteamAPICall_t DeletePublishedFile(PublishedFileId_t unPublishedFileId) {
			return NativeMethods.ISteamRemoteStorage_DeletePublishedFile(unPublishedFileId);
		}

			// enumerate the files that the current user published with this app
		public static SteamAPICall_t EnumerateUserPublishedFiles(uint unStartIndex) {
			return NativeMethods.ISteamRemoteStorage_EnumerateUserPublishedFiles(unStartIndex);
		}

		public static SteamAPICall_t SubscribePublishedFile(PublishedFileId_t unPublishedFileId) {
			return NativeMethods.ISteamRemoteStorage_SubscribePublishedFile(unPublishedFileId);
		}

		public static SteamAPICall_t EnumerateUserSubscribedFiles(uint unStartIndex) {
			return NativeMethods.ISteamRemoteStorage_EnumerateUserSubscribedFiles(unStartIndex);
		}

		public static SteamAPICall_t UnsubscribePublishedFile(PublishedFileId_t unPublishedFileId) {
			return NativeMethods.ISteamRemoteStorage_UnsubscribePublishedFile(unPublishedFileId);
		}

		public static bool UpdatePublishedFileSetChangeDescription(PublishedFileUpdateHandle_t updateHandle, string pchChangeDescription) {
			return NativeMethods.ISteamRemoteStorage_UpdatePublishedFileSetChangeDescription(updateHandle, new InteropHelp.UTF8String(pchChangeDescription));
		}

		public static SteamAPICall_t GetPublishedItemVoteDetails(PublishedFileId_t unPublishedFileId) {
			return NativeMethods.ISteamRemoteStorage_GetPublishedItemVoteDetails(unPublishedFileId);
		}

		public static SteamAPICall_t UpdateUserPublishedItemVote(PublishedFileId_t unPublishedFileId, bool bVoteUp) {
			return NativeMethods.ISteamRemoteStorage_UpdateUserPublishedItemVote(unPublishedFileId, bVoteUp);
		}

		public static SteamAPICall_t GetUserPublishedItemVoteDetails(PublishedFileId_t unPublishedFileId) {
			return NativeMethods.ISteamRemoteStorage_GetUserPublishedItemVoteDetails(unPublishedFileId);
		}

		public static SteamAPICall_t EnumerateUserSharedWorkshopFiles(CSteamID steamId, uint unStartIndex, System.Collections.Generic.IList<string> pRequiredTags, System.Collections.Generic.IList<string> pExcludedTags) {
			return NativeMethods.ISteamRemoteStorage_EnumerateUserSharedWorkshopFiles(steamId, unStartIndex, new InteropHelp.SteamParamStringArray(pRequiredTags), new InteropHelp.SteamParamStringArray(pExcludedTags));
		}

		public static SteamAPICall_t PublishVideo(EWorkshopVideoProvider eVideoProvider, string pchVideoAccount, string pchVideoIdentifier, string pchPreviewFile, AppId_t nConsumerAppId, string pchTitle, string pchDescription, ERemoteStoragePublishedFileVisibility eVisibility, System.Collections.Generic.IList<string> pTags) {
			return NativeMethods.ISteamRemoteStorage_PublishVideo(eVideoProvider, new InteropHelp.UTF8String(pchVideoAccount), new InteropHelp.UTF8String(pchVideoIdentifier), new InteropHelp.UTF8String(pchPreviewFile), nConsumerAppId, new InteropHelp.UTF8String(pchTitle), new InteropHelp.UTF8String(pchDescription), eVisibility, new InteropHelp.SteamParamStringArray(pTags));
		}

		public static SteamAPICall_t SetUserPublishedFileAction(PublishedFileId_t unPublishedFileId, EWorkshopFileAction eAction) {
			return NativeMethods.ISteamRemoteStorage_SetUserPublishedFileAction(unPublishedFileId, eAction);
		}

		public static SteamAPICall_t EnumeratePublishedFilesByUserAction(EWorkshopFileAction eAction, uint unStartIndex) {
			return NativeMethods.ISteamRemoteStorage_EnumeratePublishedFilesByUserAction(eAction, unStartIndex);
		}

			// this method enumerates the public view of workshop files
		public static SteamAPICall_t EnumeratePublishedWorkshopFiles(EWorkshopEnumerationType eEnumerationType, uint unStartIndex, uint unCount, uint unDays, System.Collections.Generic.IList<string> pTags, System.Collections.Generic.IList<string> pUserTags) {
			return NativeMethods.ISteamRemoteStorage_EnumeratePublishedWorkshopFiles(eEnumerationType, unStartIndex, unCount, unDays, new InteropHelp.SteamParamStringArray(pTags), new InteropHelp.SteamParamStringArray(pUserTags));
		}

		public static SteamAPICall_t UGCDownloadToLocation(UGCHandle_t hContent, string pchLocation, uint unPriority) {
			return NativeMethods.ISteamRemoteStorage_UGCDownloadToLocation(hContent, new InteropHelp.UTF8String(pchLocation), unPriority);
		}
	}
}