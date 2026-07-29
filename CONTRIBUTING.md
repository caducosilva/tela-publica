# Contribuindo

Obrigado por considerar contribuir. Este projeto é mantido por uma pessoa só, então revisão pode demorar um pouco.

## Como contribuir

1. **Abra uma issue** descrevendo o problema ou a ideia antes de mandar PR grande.
2. **Faça fork**, crie branch (`git checkout -b minha-mudanca`).
3. **Commits em português**, imperativo, sem emoji, seguindo [Conventional Commits](https://www.conventionalcommits.org/pt-br/):
   - `feat: adiciona suporte a captura de tela Wayland`
   - `fix: corrige vazamento de sink PipeWire ao encerrar`
   - `docs: atualiza README com instruções Fedora`
   - `refactor: separa classe MesaDeSom em módulo próprio`
   - `perf: reduz latência do chat em 200ms`
4. **Teste localmente** antes de mandar:
   ```bash
   ./tela-publica --sem-navegador --sem-tunel
   # abre outro terminal, testa no navegador
   ```
5. **PR com descrição clara** do que muda e por quê.

## Padrões do código

- Python 3.8+ (tipagem opcional, sem mypy obrigatório)
- Uma classe por responsabilidade, nomes em `PascalCase`
- Funções e variáveis em `snake_case`
- Docstrings em português nas classes públicas
- Logs técnicos em `log()`, nunca misturados com conteúdo do chat
- Paths absolutos via `os.path.expanduser` ou `pathlib.Path.home()`
- Sem dependências externas além das listadas em `DEPENDENCIAS`

## O que não aceito

- Código gerado por IA sem revisão humana
- Commits com `Co-Authored-By: Claude` ou menção a ferramentas de IA
- Emojis em commit, PR, issue ou código
- Travessão (—) no lugar de vírgula, dois pontos ou parênteses
- Paths relativos em scripts que rodam em background

## Reportar bug

Inclua:
- Distro e versão (`lsb_release -a`)
- Sessão X11 ou Wayland (`echo $XDG_SESSION_TYPE`)
- Saída de `./tela-publica --sem-navegador --sem-tunel 2>&1 | head -30`
- Passos para reproduzir

## Ideias para futuras contribuições

- Suporte a Wayland (pipewire + xdg-desktop-portal)
- Gravação local opcional (mp4 no disco)
- Várias janelas simultâneas (picture-in-picture)
- Autenticação simples no link (senha)
- Empacotamento .deb / .rpm / Flatpak / AppImage