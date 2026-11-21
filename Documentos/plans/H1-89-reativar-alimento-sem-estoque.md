# Plano: H1 (issue #89) - reativar alimento sem estoque

Sprint 5 - Extensao, Trilha 2 (Hardening), item H1. Destrava o "Esgotado preso":
hoje um alimento inativado sem estoque nao pode ser editado nem reativado.

## Problema
- `ReativarAsync` (service e repositorio) exigia `QuantidadeDisponivel > 0`; o controller
  devolvia 400.
- `AtualizarAsync` recusa alimento `Inativo`, entao nao da para repor estoque estando inativo.
- Resultado: inativar um alimento com estoque 0 o deixava preso. No #83 o front passou a
  bloquear o "Inativar" para alimento Esgotado como solucao provisoria.

## Decisao (opcao "a" do plano BRV-192)
Reativar sem estoque **nao falha**: o alimento volta como `Esgotado`. O vendedor repoe o
estoque pela edicao normal (que aceita `Esgotado`) e o alimento volta a `Disponivel`.
- Descartada a opcao "b" (permitir editar alimento inativo): abriria a edicao para itens
  inativos e mexeria em mais telas; a "a" concentra a mudanca na reativacao.

## Escopo
- **Backend:** `AlimentoService.ReativarAsync` deixa de recusar por estoque;
  `AlimentoRepository.ReativarAsync` define o status no proprio UPDATE
  (`QuantidadeDisponivel > 0 ? Disponivel : Esgotado`); mensagem do 400 do controller deixa
  de citar estoque (agora so falha se o alimento nao for do vendedor).
- **Testes:** `Reativar_SemEstoque_NaoReativa` vira `Reativar_SemEstoque_DelegaAoRepositorio`;
  novo `Inativar_AlimentoEsgotado_Inativa`.
- **Frontend:** `BotaoStatusAlimento` remove a trava e o aviso de "sem estoque"; a mensagem
  de erro do 400 ao reativar deixa de mandar repor estoque.
- Sem migration, sem endpoint novo, sem mudanca de contrato (mesmas rotas e codigos).

## Fora do escopo
- Editar alimento inativo.
- Bazar (o `BotaoExcluirItemBazar` continua igual).
- Teste de integracao do repositorio (exige `UNIHUB_TEST_DB`); a regra do status fica
  coberta pelo teste manual abaixo.

## Passos
1. Backend: service, repositorio e mensagem do controller.
2. Testes unitarios do service.
3. Frontend: remover trava e aviso.
4. `dotnet build`, testes unitarios, `npm run lint`, `npm run build`.
5. Teste manual no site.

## Criterios de aceite
- [ ] Alimento Esgotado pode ser inativado (botao habilitado, sem aviso).
- [ ] Reativar alimento inativo sem estoque o devolve como Esgotado (sem erro).
- [ ] Reativar alimento inativo com estoque o devolve como Disponivel.
- [ ] Repondo o estoque pela edicao, o alimento Esgotado volta a Disponivel e reaparece na
      Alimentacao.
- [ ] `dotnet build` e testes unitarios limpos; `npm run lint` e `npm run build` limpos.
