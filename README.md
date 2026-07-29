# tela-publica

Transmite uma janela do seu computador para alguém assistir pelo navegador, de
qualquer lugar, sem instalar nada do outro lado.

## Por que existe

Eu queria assistir um filme junto com minha namorada, cada um na sua casa. As
opções eram todas ruins: programa de chamada de vídeo comprime a imagem até
virar borrão e corta o áudio quando ninguém fala, serviço de terceiro grava o
que você mostra, e abrir porta no roteador não funciona quando a operadora usa
CGNAT.

Este projeto resolve com um arquivo só. Você abre, clica na janela do player,
manda o link. Do outro lado é só abrir no navegador do celular ou do
computador. Nada para instalar, nada para configurar, nada gravado.

## Recursos

- **Imagem sempre igual.** 1920x1080 a 30 fps. A janela entra centralizada
  nesse quadro, com tarja preta no que sobrar, então a resolução nunca muda no
  meio da transmissão e o player de quem assiste não fica reconectando.
- **Som apenas da janela escolhida.** O áudio do programa é desviado para um
  destino próprio no PipeWire, sem sair das suas caixas. Notificação, música e
  qualquer outro som do computador não vão junto.
- **Microfone com botão de mudo.** Ligue só quando quiser falar com quem
  assiste, para avisar que travou. Liga e desliga na hora, sem cortar a imagem.
- **Chat que não guarda nada.** As mensagens existem apenas na memória e somem
  sozinhas um minuto depois. Cada pessoa escolhe um apelido e pode mandar uma
  mensagem a cada 5 segundos, o que evita enchente de texto.
- **Link público sem mexer no roteador.** Usa o cloudflared para criar um
  endereço HTTPS temporário. Funciona mesmo com CGNAT.
- **Qualidade que se ajusta sozinha.** Se a conexão de quem assiste apertar,
  cai para 720p e depois 480p. Depois de 3 minutos estável, volta para 1080p.
- **Cuida da janela minimizada.** O X11 não entrega imagem nenhuma de janela
  minimizada. O programa restaura a janela ao começar e, se você minimizar no
  meio, pausa avisando e retoma sozinho quando ela volta.
- **Janela coberta funciona.** Pode deixar o filme atrás de outras janelas. Com
  o compositor ligado, a captura pega o conteúdo real da janela.
- **Feito para o celular.** Botão de tela cheia que já deita a tela no Android.
  Toque duplo no vídeo faz o mesmo.

## Começo rápido

### Windows 10/11

Requisitos: Python 3.8+ e FFmpeg no PATH (`winget install Gyan.FFmpeg`).
O cloudflared já pode estar instalado; se não, rode com `--instalar-cloudflared`.

```powershell
cd $env:USERPROFILE\tela-publica
.\tela-publica.bat
```

Ou direto:

```powershell
python .\tela-publica
```

No Windows a captura usa **gdigrab** (tela/janela) e áudio via **DirectShow**.
Som do sistema só entra se existir *Stereo Mix* / *Cable Output*; o microfone
liga pelo painel. O encoder GPU (NVENC/AMF/QSV) é usado só se passar no teste;
senão cai para libx264.

### Linux

Para quem só quer usar, são três comandos:

```bash
sudo apt install ffmpeg wmctrl x11-utils x11-xserver-utils pulseaudio-utils pipewire-bin
curl -sSL https://raw.githubusercontent.com/caducosilva/tela-publica/main/tela-publica -o tela-publica
chmod +x tela-publica && ./tela-publica
```

O painel abre sozinho no navegador. Se quiser o link que funciona fora da sua
casa, rode uma vez `./tela-publica --instalar-cloudflared`.

## Requisitos

Roda em **Windows 10/11** e em **Linux com sessão X11**. No Linux, Wayland não
serve, porque a captura de janela usa o X11. Na tela de login, clique na
engrenagem e escolha X11 (às vezes aparece como Xorg). O programa avisa se você
estiver em Wayland.

| Sistema | Dependências |
|---|---|
| Windows 10/11 | Python 3.8+, FFmpeg (`winget install Gyan.FFmpeg`), cloudflared opcional |
| Ubuntu, Debian, Mint, Pop!_OS | `ffmpeg wmctrl x11-utils x11-xserver-utils pulseaudio-utils pipewire-bin` |
| Arch, Manjaro | `ffmpeg wmctrl xorg-xwininfo xorg-xrandr libpulse pipewire` |
| Fedora | `ffmpeg wmctrl xorg-x11-utils xorg-x11-server-utils pulseaudio-utils pipewire` |

Python 3.8 ou mais novo, sem nenhuma biblioteca de fora. No Linux, o som
separado por programa depende do PipeWire.

Quem assiste não precisa de nada além de um navegador atual.

## Uso

1. Abra o programa. O painel aparece no navegador.
2. Clique na janela que quer mostrar. A lista se atualiza sozinha a cada 10
   segundos.
3. Clique em **Comecar**.
4. Copie o link e mande para quem vai assistir. Tem botão de copiar e de enviar
   pelo WhatsApp.
5. Se precisar falar, escolha o microfone e clique em **Microfone desligado**
   para colocá-lo no ar.

Fechar a janela do painel encerra tudo e libera a porta.

Opções de linha de comando:

```bash
./tela-publica --sem-navegador          # não abre o navegador sozinho
./tela-publica --sem-tunel              # só rede local, sem link público
./tela-publica --porta 9000             # porta preferida para quem assiste
./tela-publica --instalar-cloudflared   # baixa o cloudflared
```

## Problemas conhecidos

| Sintoma | Causa | Solução |
|---|---|---|
| Aviso de Wayland ao abrir | A sessão não é X11 e a captura de janela não funciona nela | Troque para X11 na engrenagem da tela de login |
| "Essa janela está minimizada" | O X11 recusa a captura de janela minimizada e devolve zero quadros | Restaure a janela. O programa tenta fazer isso sozinho e retoma quando ela volta |
| A janela não aparece na lista | Menor que 120 por 120 pixels, ou é painel do sistema | Abra a janela em tamanho normal |
| "Esperando esse programa tocar algum som" | O programa ainda não abriu um fluxo de áudio | Dê play. Alguns programas só criam o fluxo quando começam a tocar |
| Travando na casa de quem assiste | A taxa não cabe no seu envio de internet | Ele cai sozinho para 720p ou 480p. Meça seu upload em fast.com e lembre que o vídeo usa no máximo metade dele |
| O link do túnel não abre no seu próprio PC | O sistema guardou uma resposta negativa de DNS de antes do túnel subir | Rode `resolvectl flush-caches` |

## Estrutura

Arquivo único, cerca de 2260 linhas, para poder baixar e rodar sem instalação.

| Parte | O que faz |
|---|---|
| `MesaDeSom` | Decide o que vai ao ar no áudio, ligando o programa e o microfone num destino próprio do PipeWire |
| `Captura` | Toca o ffmpeg, corta o vídeo em pedaços e cuida da janela minimizada |
| `Adaptador` | Sobe e desce a qualidade olhando como quem assiste está indo |
| `Chat` | Mensagens na memória, com prazo de validade e limite de envio |
| `Tunel` | Liga o cloudflared e pega o endereço público |
| `Manipulador` | Servidor HTTP do painel e da página de quem assiste |

O vídeo sai em MP4 fragmentado por HTTP e é montado no navegador com Media
Source Extensions. Isso dá cerca de 2 segundos de atraso, mantém a qualidade
cheia e não exige nenhuma biblioteca no lado de quem assiste.

## Segurança

- Tudo roda na sua máquina. Nenhum vídeo passa por servidor de terceiro, com a
  ressalva de que o túnel do Cloudflare é o caminho por onde o tráfego trafega
  quando você usa o link público.
- O link de quem assiste carrega um token aleatório. Sem ele, a resposta é 403.
- O painel de controle escuta apenas em 127.0.0.1 e tem chave própria, então
  nem com o túnel aberto alguém de fora alcança seus controles.
- O chat não grava nada em disco e o conteúdo das mensagens nunca entra no
  registro do programa.
- O microfone só entra na transmissão quando você liga, e o botão mostra o
  estado o tempo todo.

Lembre que o link público, enquanto estiver de pé, é acessível por qualquer
pessoa que tenha o endereço completo. Encerre quando terminar.

## Autor

tela-publica, criado por **caducosilva**. Contato: abobicarlo@gmail.com

Outros projetos em https://github.com/caducosilva

## Apoie

Se este projeto te ajudou e você quiser contribuir:

**PIX (chave aleatória):** `f74458dc-2a36-49bd-9250-1cef4365ebb8`

Titular: Carlos Eduardo, Mogi das Cruzes.

## Licença

MIT. Veja o arquivo [LICENSE](LICENSE).
