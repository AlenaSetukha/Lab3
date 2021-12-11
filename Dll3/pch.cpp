// pch.cpp: файл исходного кода, соответствующий предварительно скомпилированному заголовочному файлу

#include "pch.h"
#include "mkl.h"
#include <string.h>
#include <time.h>
#include <stdio.h>
#include <cmath>
#include <iostream>
#include <chrono>
#include <ctime> 
// При использовании предварительно скомпилированных заголовочных файлов необходим следующий файл исходного кода для выполнения сборки.


extern "C" _declspec(dllexport)
void Global_func(double* v, int n, double* res_mkl, double* res, char* mode, double& v_time, int& ret)
{
    try
    {
        //------------------------------Using MKL----------------------------------
        MKL_INT64 k = 0;

        if (!strcmp(mode, "VML_HA"))
        {
            k = VML_HA;
        }
        if (!strcmp(mode, "VML_LA"))
        {
            k = VML_LA;
        }
        if (!strcmp(mode, "VML_EP"))
        {
            k = VML_EP;
        }
        //n - number of elements, v - array, res - result, k - mode option
        auto start1 = std::chrono::system_clock::now();
        vmdTan(n, v, res_mkl, k);
        auto end1 = std::chrono::system_clock::now();
        std::chrono::duration<double> elapsed_seconds1 = end1 - start1;
        //----------------------------Without MKL----------------------------------
        auto start2 = std::chrono::system_clock::now();
        for (int i = 0; i < n; i++)
        {
            res[i] = tan(v[i]);
        }
        auto end2 = std::chrono::system_clock::now();
        std::chrono::duration<double> elapsed_seconds2 = end2 - start2;
        //--------------------------------Result-------------------------------------
        v_time = elapsed_seconds1.count() / elapsed_seconds2.count();
        ret = 0;
    }
    catch (...)
    {
        ret = -1;
    }
}