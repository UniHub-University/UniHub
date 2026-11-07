# Plano — BRV-192: Gestão de alimentos (reativar e texto "Inativar") — Frente 3, Sprint 5

## Objetivo
No painel "Meus produtos", o vendedor deve conseguir **reativar** um alimento inativado, e o botão de alimento deve dizer **"Inativar"** (ação reversível) em vez de "Excluir" (que sugere perda definitiva). Também deve ser possível **inativar um alimento esgotado** (produto que acabou e não será reposto) sem deixá-lo preso.

## Escopo
- **Inclui (front):** componente de botão para alimento que alterna entre "Inativar anúncio" e "Reativar anúncio" conforme o status; uso dele em `MeusProdutos.jsx` para produtos do tipo `lanche`; mensagens claras de confirmação e de erro; estilos do botão "Reativar".
- **Inclui (backend, regra de reativação):** reativar um alimento **sem estoque** passa a ser permitido e o devolve como `Esgotado` (com estoque, continua voltando como `Disponivel`); ajuste da mensagem do controller e dos testes de unidade.
- **NÃO inclui:** migration (nenhuma mudança de schema); permitir **editar** alimento inativo; mudar o botão de excluir do Bazar (`BotaoExcluirItemBazar` continua igual); reposição de estoque; outras telas.

## Decisões de design
- **Componente novo** `BotaoStatusAlimento.jsx`, em vez de alterar o `BotaoExcluirItemBazar`, para não arriscar regressão no Bazar, que exclui de verdade.
- **Regra de exibição:** status `2` (Inativo) mostra "Reativar anúncio"; qualquer outro status mostra "Inativar anúncio".
- **Confirmação ao inativar:** "Inativar “nome”? Ele deixa de aparecer para os compradores, mas você pode reativá-lo depois." Reativar não pede confirmação (é seguro).
- **Após a ação:** a lista é recarregada do servidor, e o alimento permanece na lista com o novo status (antes, o inativado era removido da tela).
- **Problema encontrado no teste:** o backend recusava reativar alimento com estoque 0, e alimento inativo não pode ser aberto nem editado (`ObterPorIdAsync` e `AtualizarAsync` recusam `Inativo`). Inativar um alimento esgotado o deixava **preso**. Uma trava no front (desabilitar "Inativar" quando esgotado) foi usada como contorno inicial e será removida.
- **Regra de reativação:** `AlimentoRepository.ReativarAsync` passa a definir o status por UPDATE condicional atômico: `Disponivel` se `QuantidadeDisponivel > 0`, senão `Esgotado`. O `AlimentoService.ReativarAsync` deixa de recusar estoque 0 (continua exigindo posse, pelo vendedor do token). Alimento esgotado nunca aparece na listagem pública (que exige `Disponivel` e estoque > 0), então reativá-lo sem estoque não o expõe aos compradores.
- **Segurança:** a posse do alimento continua validada no backend (vendedor vem do token); o front só chama os endpoints existentes.

## Passos de implementação
1. Plano (este arquivo).
2. Componente `BotaoStatusAlimento.jsx` (inativar/reativar, confirmação, erros).
3. `MeusProdutos.jsx` usa o componente para `lanche` e recarrega a lista; estilos em `MeusProdutos.css`.
4. Backend: reativar alimento sem estoque devolve `Esgotado` (repositório, service, mensagem do controller) + testes.
5. Front: remover a trava de "Inativar" em alimento esgotado.

## Critérios de aceite
- [ ] Alimento ativo mostra "Inativar anúncio"; ao confirmar, fica Inativo e o botão vira "Reativar anúncio"
- [ ] "Reativar anúncio" com estoque volta o alimento para Disponível e ele reaparece na Alimentação
- [ ] Alimento esgotado pode ser inativado e reativado (volta Esgotado, sem aparecer na Alimentação), e depois pode ser editado para repor o estoque
- [ ] Reativar alimento de outro vendedor continua sendo recusado
- [ ] O botão de excluir do Bazar continua com o texto e o comportamento atuais
- [ ] `dotnet build`, `dotnet test` (unitários) e `npm run build` limpos
- [ ] Testado no site

## Pendências / dúvidas em aberto
- Permitir **editar** alimento inativo (sem precisar reativar antes) fica como melhoria futura: exigiria alterar o detalhe e a atualização do módulo de Alimentação.
