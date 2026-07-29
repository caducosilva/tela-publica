@echo off
chcp 65001 >nul
title tela-publica
cd /d "%~dp0"

where python >nul 2>&1
if errorlevel 1 (
  echo Python nao encontrado. Instale em https://www.python.org/downloads/
  pause
  exit /b 1
)

where ffmpeg >nul 2>&1
if errorlevel 1 (
  echo FFmpeg nao esta no PATH. Tentando mesmo assim...
)

echo Iniciando tela-publica...
python "%~dp0tela-publica" %*
echo.
echo Encerrado. Codigo: %ERRORLEVEL%
pause
