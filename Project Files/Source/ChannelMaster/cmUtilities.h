
extern __declspec (dllexport) void print_cmbuff_parameters(const char* filename, int id);

extern __declspec (dllexport) void WriteAudio(double seconds, int rate, int size, double* indata, int mode);

extern __declspec (dllexport) void WriteCharFiles(int seconds, int rate, unsigned char* indata, int num_ddcs);