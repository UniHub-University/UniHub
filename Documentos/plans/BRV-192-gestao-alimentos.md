\# Plano — BRV-192: Gestão de alimentos (reativar e texto "Inativar") — Frente 3, Sprint 5



\## Objetivo

No painel "Meus produtos", o vendedor deve conseguir \*\*reativar\*\* um alimento inativado, e o botão de alimento deve dizer \*\*"Inativar"\*\* (ação reversível) em vez de "Excluir" (que sugere perda definitiva).



\## Escopo

\- \*\*Inclui:\*\* componente de botão para alimento que alterna entre "Inativar anúncio" e "Reativar anúncio" conforme o status; uso dele em `MeusProdutos.jsx` para produtos do tipo `lanche`; mensagens claras de confirmação e de erro; estilos do botão "Reativar".

\- \*\*NÃO inclui:\*\* mudança de backend ou de migration (os endpoints `DELETE /api/alimentos/{id}` e `PATCH /api/alimentos/{id}/reativar` já existem); mudar o botão de excluir do Bazar (`BotaoExcluirItemBazar` continua igual); reposição de estoque; outras telas.



\## Decisões de design

\- \*\*Componente novo\*\* `BotaoStatusAlimento.jsx`, em vez de alterar o `BotaoExcluirItemBazar`, para não arriscar regressão no Bazar, que exclui de verdade.

\- \*\*Regra de exibição:\*\* status `2` (Inativo) mostra "Reativar anúncio"; qualquer outro status mostra "Inativar anúncio".

\- \*\*Confirmação ao inativar:\*\* "Inativar “nome”? Ele deixa de aparecer para os compradores, mas você pode reativá-lo depois." Reativar não pede confirmação (é seguro).

\- \*\*Após a ação:\*\* a lista é recarregada do servidor, e o alimento permanece na lista com o novo status (antes, o inativado era removido da tela).

\- \*\*Reativar sem estoque:\*\* o backend devolve 400 se a quantidade for 0; a tela mostra a mensagem do servidor e orienta a repor o estoque pela edição.

\- \*\*Segurança:\*\* a posse do alimento já é validada no backend (o vendedor vem do token); o front só chama os endpoints existentes.



\## Passos de implementação

1\. Plano (este arquivo).

2\. Componente `BotaoStatusAlimento.jsx` (inativar/reativar, confirmação, erros).

3\. `MeusProdutos.jsx` usa o componente para `lanche` e recarrega a lista; estilos em `MeusProdutos.css`.



\## Critérios de aceite

\- \[ ] Alimento ativo mostra "Inativar anúncio"; ao confirmar, fica Inativo e o botão vira "Reativar anúncio"

\- \[ ] "Reativar anúncio" volta o alimento para Disponível e ele reaparece na Alimentação

\- \[ ] Reativar com estoque 0 mostra o erro e não altera o status

\- \[ ] O botão de excluir do Bazar continua com o texto e o comportamento atuais

\- \[ ] `npm run build` limpo

\- \[ ] Testado no site



\## Pendências / dúvidas em aberto

\- Nenhuma.

