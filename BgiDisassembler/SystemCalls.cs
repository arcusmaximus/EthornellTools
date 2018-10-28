namespace Arc.Ddsi.BgiDisassembler
{
    internal partial class Disassembler
    {
        private static readonly string[] SystemCalls80 =
        {
            "void srand(int value)",                                // 00
            "int rand()",                                           // 01
            "int rand(int max)",                                    // 02
            null,                                                   // 03
            "int getTickCount()",                                   // 04
            "bool getPerformanceCounter(long* ptr)",                // 05
            null,                                                   // 06
            null,                                                   // 07
            null,                                                   // 08
            null,                                                   // 09
            null,                                                   // 0A
            null,                                                   // 0B
            "void getLocalTime(void* ptr)",                         // 0C
            "int, int getMemoryInfo()",                             // 0D
            null,                                                   // 0E
            null,                                                   // 0F
            null,                                                   // 10
            "bool getAsyncKeyState(int key)",                       // 11
            null,                                                   // 12
            null,                                                   // 13
            null,                                                   // 14
            null,                                                   // 15
            null,                                                   // 16
            null,                                                   // 17
            null,                                                   // 18
            null,                                                   // 19
            null,                                                   // 1A
            null,                                                   // 1B
            null,                                                   // 1C
            null,                                                   // 1D
            null,                                                   // 1E
            null,                                                   // 1F
            "void* alloc(int size)",                                // 20
            "bool free(void* ptr)",                                 // 21
            null,                                                   // 22
            null,                                                   // 23
            null,                                                   // 24
            null,                                                   // 25
            null,                                                   // 26
            null,                                                   // 27
            "int createDirectory(char* pPath)",                     // 28
            "int removeDirectory(char* pPath)",                     // 29
            "bool directoryExists(char* pPath)",                    // 2A
            null,                                                   // 2B
            "int getFileAttributes(char* path)",                    // 2C
            "int setFileAttributes(char* path, int attributes)",    // 2D
            null,                                                   // 2E
            "int copyFile(char* pTo, char* pFrom)",                 // 2F
            "int loadFile(void* pBuffer, char* pszArchiveFileName, char* pszFileName)",                                 // 30
            "int loadFileSection(void* buffer, char* pszArchiveFileName, char* pszFileName, int offset, int length)",   // 31
            null,                                                   // 32
            "int deleteFile(char* pszDirectory, char* pszFile)",    // 33
            "bool fileExists(char* pszArchiveFileName, char* pszFileName)",  // 34
            "int getFileSize(char* pszArchiveFileName, char* pszFileName)",  // 35
            null,                                                   // 36
            null,                                                   // 37
            null,                                                   // 38
            null,                                                   // 39
            null,                                                   // 3A
            null,                                                   // 3B
            null,                                                   // 3C
            null,                                                   // 3D
            null,                                                   // 3E
            null,                                                   // 3F
            "void* loadProgram(char* pszArchiveFileName, char* pszFileName)",   // 40
            null,                                                   // 41
            null,                                                   // 42
            null,                                                   // 43
            null,                                                   // 44
            null,                                                   // 45
            null,                                                   // 46
            null,                                                   // 47
            null,                                                   // 48
            null,                                                   // 49
            null,                                                   // 4A
            null,                                                   // 4B
            null,                                                   // 4C
            null,                                                   // 4D
            null,                                                   // 4E
            null,                                                   // 4F
            null,                                                   // 50
            null,                                                   // 51
            null,                                                   // 52
            null,                                                   // 53
            null,                                                   // 54
            null,                                                   // 55
            null,                                                   // 56
            null,                                                   // 57
            null,                                                   // 58
            null,                                                   // 59
            null,                                                   // 5A
            null,                                                   // 5B
            null,                                                   // 5C
            null,                                                   // 5D
            null,                                                   // 5E
            "void yield()"                                          // 5F
        };
    }
}
