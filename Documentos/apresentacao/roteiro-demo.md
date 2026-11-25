# Roteiro de Demo — UniHub (12 min)

> Demonstração ao vivo. Objetivo: 3 módulos + Moderação + o diferencial (tempo real),
> numa narrativa fluida. Tempo-alvo: ~12 min (dentro do bloco de demo).

## ⚙️ Antes (checklist de 5 min antes)
- [ ] **Aquecer o deploy** (pingar `/health`). NÃO deixe o cold start acontecer na banca.
- [ ] 3 abas logadas: **comprador** / **vendedor** / **admin**.
- [ ] 1 produto no Bazar com **estoque baixo** (1–2 unidades) para a cena do tempo real.
- [ ] 1 solicitação de Ação Solidária já criada (ou criar na hora).
- [ ] Rodar o fluxo inteiro UMA vez antes, em produção.

## 🎬 Roteiro (com tempos)

### 1. Entrada — confiança (0:30)
- Home (`/`): banner dos 3 módulos + vitrines. **Login** Google `@unifesp.br`.
- **Fala:** "Acesso restrito a e-mails institucionais — ambiente só de universitários."

### 2. Bazar + TEMPO REAL — o "uau" (3:00) ·
- Aba A (comprador): `/bazar` → item com estoque baixo.
- Posicionar Aba B (vendedor) em `/vendedor/produtos`, mesmo item, visível.
- Aba A: **reservar/comprar**.
- 💥 Apontar Aba B: estoque **atualiza sozinho, sem refresh**; se zerar, vira "Esgotado" ao vivo.
- **Fala:** "O painel do vendedor atualizou em tempo real, sem recarregar. É o diferencial técnico — já explico a engenharia."

### 3. Alimentação — 2º módulo + pagamento (1:30)
- `/alimentacao` → um lanche. Mostrar **foto do produto** + **bloco do vendedor com PIX** (copiar).
- **Fala:** "O comprador vê a chave PIX no anúncio pra pagar — pensado pra realidade do campus."

### 4. Ação Solidária — LGPD em ação (2:30)
- `/solidariedade` → "Preciso de apoio" → criar solicitação (anônima).
- Trocar p/ voluntário: `/solidariedade/sugestoes` → aceitar.
- Mostrar identidade **anônima** (código público) até o solicitante **revelar**.
- **Fala:** "Identidade protegida por padrão, pseudonimizada. Só revelada com autorização do próprio solicitante. Privacy by design."

### 5. Moderação — governança (2:00)
- Como usuário: detalhe de produto ou perfil → **"Denunciar"** → motivo + justificativa.
- Trocar p/ admin: `/moderacao` → ver a denúncia → **mudar status** / **suspender usuário**.
- (Opcional) `/minhas-denuncias`: o denunciante acompanha o desfecho.
- **Fala:** "Governança: qualquer um denuncia, o admin modera. Ciclo completo."

### 6. Fecho (0:30)
- Voltar à Home. "Três módulos, moderação, tempo real, deploy no ar — tudo integrado."

## 🧯 Planos B (se algo falhar)
- **Cold start / lentidão:** `/health` já aquecido; se travar, narrar enquanto carrega.
- **Tempo real não atualizar:** o painel mostra o estado inicial via REST — refresh manual e seguir; mencionar reconexão automática.
- **Ambiente cair:** **vídeo gravado da demo** como backup (gravar no ensaio). Salva a apresentação.
- **Admin não abrir painel:** confirmar ANTES que a conta foi promovida no banco **e relogada**.

## Divisão (quem faz)
- 1 pessoa **conduz os cliques**, 1 pessoa **narra** (falas acima). Combinar quem troca de aba na cena #2 — coordenação é o que faz o tempo real brilhar.
