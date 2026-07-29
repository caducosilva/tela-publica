# tela-publica

Transmite uma janela do seu computador para alguém assistir pelo navegador, de qualquer lugar, sem instalar nada do outro lado.

## Por que existe

Compartilhar a tela no Linux costuma exigir configurar servidor, abrir porta no roteador, lidar com IP público, ou depender de serviços de terceiros que gravam tudo. Este projeto resolve com um único arquivo Python: abre o painel no navegador, escolhe a janela, envia o link. Quem assiste só abre no celular ou computador — nada para instalar, nada fica gravado.

## Recursos

- **Imagem fixa** — Sempre 1920×1080 a 30 fps. A janela entra centralizada com tarjas pretas se precisar, então a resolução nunca muda no meio da transmissão e o player não reconecta.
- **Som só da janela** — Roteia no PipeWire apenas o áudio do programa escolhido. Você continua ouvindo tudo normal. Microfone entra só quando você liga, para avisar quem assiste.
- **Chat efêmero** — Mensagens só na memória, somem sozinhas após 1 minuto. Nickname por pessoa, limite de 1 mensagem a cada 5 segundos, nada vai para disco.
- **Link público sem mexer no roteador** — Usa `cloudflared` para criar túnel HTTPS automático. Funciona de qualquer lugar.
- **Qualidade adaptativa** — Cai sozinha (1080p → 720p → 480p) se a conexão de quem assiste não aguenta. Sobe de volta após 3 minutos de calmaria.
- **Janela minimizada** — Restaura sozinha ao iniciar. Se minimizar no meio, pausa e avisa; volta sozinha quando você restaura.
- **Janela coberta** — Captura certo mesmo com outras janelas por cima (compositor XFCE testado).
- **Mobile** — Botão de tela cheia que trava em paisagem no Android. Toque duplo no vídeo faz o mesmo. No iPhone o giro é nativo do player.
- **Interface amigável** — Lista de janelas atualiza sozinha a cada 10 segundos. Botões com resposta tátil (mínimo 40 px). Painel não pisca mais. Tema claro/escuro automático.

## Começo rápido

### Requisitos

| Sistema | Pacotes necessários |
|---------|---------------------|
| Ubuntu / Debian / Mint / Pop!_OS | `ffmpeg wmctrl x11-utils x11-xserver-utils pulseaudio-utils pipewire-bin` |
| Arch / Manjaro | `ffmpeg wmctrl xorg-xwininfo xorg-xrandr libpulse pipewire` |
| Fedora | `ffmpeg wmctrl xorg-x11-utils xorg-x11-server-utils pulseaudio-utils pipewire` |

**Importante:** Precisa de sessão **X11** (Xorg), não Wayland. Na tela de login, clique na engrenagem e escolha "X11" ou "Xorg".

### Instalação

```bash
# Baixa o script único
curl -sSL https://raw.githubusercontent.com/caducosilva/tela-publica/main/tela-publica -o tela-publica
chmod +x tela-publica

# Instala dependências (Ubuntu/Mint/Debian)
sudo apt install ffmpeg wmctrl x11-utils x11-xserver-utils pulseaudio-utils pipewire-bin

# Opcional: link público automático (cloudflared)
./tela-publica --instalar-cloudflared
```

### Uso

```bash
# Executa e abre o painel no navegador
./tela-publica

# Sem abrir navegador (para servidores/ssh)
./tela-publica --sem-navegador

# Sem túnel público (só rede local)
./tela-publica --sem-tunel

# Porta personalizada
./tela-publica --porta 9000
```

1. Abre o painel no navegador (ou acesse `http://localhost:8787` com a chave mostrada no terminal)
2. Clique na janela que quer transmitir
3. Escolha microfone se quiser falar (opcional)
4. Clique **Começar**
5. Copie o link e envie para quem vai assistir

## Problemas conhecidos

| Sintoma | Causa | Solução |
|---------|-------|---------|
| "Sua sessão está em Wayland" | Login usa Wayland | Troque para X11/Xorg na tela de login |
| Janela não aparece na lista | Muito pequena (<120×120) ou painel do sistema | Abra a janela em tamanho normal |
| Sem som | PipeWire não expõe o PID do app | O programa tenta achar pelo nome; se falhar, avisa no painel |
| `cloudflared: command not found` | Não instalou | Rode `./tela-publica --instalar-cloudflared` |
| Qualidade cai sozinha | Conexão de quem assiste apertou | Normal. Sobe sozinha quando estabiliza (3 min) |

## Estrutura do código

```
tela-publica/           # Script principal (Python 3)
├── tela-publica        # Arquivo único, ~2200 linhas
```

Tudo em um arquivo para facilitar distribuição: `chmod +x tela-publica && ./tela-publica`.

Principais classes:
- `MesaDeSom` — Roteamento PipeWire (janela + microfone)
- `Captura` — ffmpeg + x11grab, qualidade adaptativa, vigia de janela
- `Chat` — Memória, TTL 60s, rate limit 5s
- `Tunel` — cloudflared subprocess
- `Contexto` — Orquestra tudo, salva config, health check
- `Manipulador` — HTTP server (painel + viewer + API)

## Segurança

- Roda 100% local. O túnel Cloudflare expõe só a porta HTTP local via HTTPS público.
- Token aleatório no link (`/v/<token>`) — sem token, não assiste.
- Painel de controle protegido por chave separada (`chave_painel`).
- Nenhum log de conteúdo do chat. Áudio do microfone só sai quando você liga.
- Fechar a aba do painel encerra tudo (com 6s de tolerância para F5).

## Autor e licença

**tela-publica** · criado por **caducosilva** · contato: abobicarlo@gmail.com

Licença MIT — veja [LICENSE](LICENSE).

---

### Apoie o projeto

Se quiser ajudar a manter esse e outros projetos abertos:

**PIX (chave aleatória):** `f74458dc-2a36-49bd-9250-1cef4365ebb8`  
**Titular:** Carlos Eduardo  
**Cidade:** Mogi das Cruzes

Ou acesse o perfil GitHub para ver outros projetos: https://github.com/caducosilva