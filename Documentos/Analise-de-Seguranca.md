# Análise de Segurança — UniHub

> Varredura de segurança feita antes da entrega final. Registra o que foi verificado,
> o que está sólido e os pontos a corrigir (cada um vira uma issue). Documento vivo:
> atualizar conforme os itens forem resolvidos.

## Resumo
A higiene de segurança do projeto está acima da média para um trabalho acadêmico.
Nenhuma vulnerabilidade crítica de escalonamento de privilégio, injeção ou vazamento
de segredo foi encontrada. Os ajustes abaixo são **hardening** (endurecimento), não
correção de falhas exploráveis — mas valem para a entrega final.

## ✓ Verificado e sólido
- **Segredos:** `appsettings.json` versionado só tem placeholders
  (`SET_VIA_ENV_OR_USER_SECRETS`); segredos reais via env/user-secrets (Render).
- **SQL Injection:** sem SQL raw — todo acesso via EF LINQ (parametrizado).
- **Escalonamento de privilégio (auto-promoção a Admin):** sem brecha. Usuário nasce
  `Aluno`; nenhum DTO de entrada aceita `Role`; nenhum endpoint seta role; JWT com
  role=Admin forjado é rejeitado (assinatura validada). Role só muda no banco (Neon).
- **IDOR / posse:** dono de recurso derivado do token, não de input.
- **Upload de imagem:** valida extensão **e** magic bytes (assinatura binária) + tamanho
  máximo — impede renomear arquivo arbitrário para `.jpg`.
- **Logging sensível:** `EnableSensitiveDataLogging` restrito a Development (LGPD).
- **JWT:** assinatura/issuer/audience/lifetime validados; expiração configurável (120 min).
- **DDoS (borda):** tráfego passa por Cloudflare (mitigação L3/L4 automática) + Render.

## 🟡 Pontos a corrigir (viram issues)

### S1 — Swagger exposto em produção ● prioridade
`app.UseSwagger()`/`UseSwaggerUI()` rodam sem guarda de ambiente (há um `// TODO` no
código). Em produção, expõe o mapa completo da API (endpoints, DTOs, parâmetros) —
reconhecimento gratuito para um atacante.
**Correção:** envolver em `if (app.Environment.IsDevelopment())`.

### S2 — Sem handler global de exceção 🟡
Não há `UseExceptionHandler`/`ProblemDetails`. Uma exceção não tratada pode vazar
stack trace na resposta (information disclosure).
**Correção:** `UseExceptionHandler` que, em produção, retorna erro genérico (500) sem
detalhes internos.

### S3 — Rate limiting ausente 🟡
Sem throttling no login nem em endpoints sensíveis (ex.: `POST /denuncias`). Permite
abuso/brute-force.
**Correção:** `AddRateLimiter` nativo do .NET no login e no criar-denúncia.

## ○ Menor prioridade / consciente (registrado, não corrigir agora)
- **Suspensão não derruba sessão ativa:** `IsActive` só é checado no login; usuário já
  logado segue com token válido até expirar. Mitigação rigorosa = middleware por
  requisição. Melhoria, não brecha.
- **`RequireHttpsMetadata = false`:** OK — TLS terminado na borda (Cloudflare/Render);
  app recebe HTTP interno. Confirmado como esperado.
- **Security headers (CSP, X-Frame-Options):** podem ser injetados na borda; não
  crítico para o escopo acadêmico.

## Resiliência / Rollback
- **Rollback de deploy:** o Render permite voltar a um deploy anterior (manual, pelo
  painel) — cada deploy vem de um commit da `main`. Procedimento a documentar no
  `DEPLOY.md`.
- **Rollback de dados:** EF Core é transacional por operação (falha em `SaveChanges`
  não commita); backup/restore é função do Neon (branching / point-in-time no plano pago).
- **DDoS:** defesa é de infraestrutura (Cloudflare + Render), não de código. Rate
  limiting (S3) protege contra abuso de endpoint, não contra ataque volumétrico.

## Encaminhamento
- S1, S2, S3 → issues criadas, alocadas na Trilha 2 (Hardening) da extensão da Sprint 5.
- Rollback do Render → documentar no `DEPLOY.md`.
