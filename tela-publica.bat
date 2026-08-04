@echo off
chcp 65001 >nul
title Transmitir Tela
cd /d "%~dp0"

set "PY="
where py >nul 2>&1 && set "PY=py -3"
if not defined PY if exist "%LocalAppData%\Programs\Python\Python312\python.exe" set "PY=%LocalAppData%\Programs\Python\Python312\python.exe"
if not defined PY if exist "%LocalAppData%\Programs\Python\Python311\python.exe" set "PY=%LocalAppData%\Programs\Python\Python311\python.exe"
if not defined PY where python >nul 2>&1 && set "PY=python"

if not defined PY (
  echo Python nao encontrado. Instale em https://www.python.org/downloads/
  pause
  exit /b 1
)

where ffmpeg >nul 2>&1
if errorlevel 1 (
  echo FFmpeg nao esta no PATH. Tentando mesmo assim...
)

echo Iniciando Transmitir Tela...
%PY% "%~dp0tela-publica" %*
echo.
echo Encerrado. Codigo: %ERRORLEVEL%
pause
