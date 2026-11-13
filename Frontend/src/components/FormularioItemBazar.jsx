import { useRef, useState } from 'react';
import { mensagemErroItem, normalizarEnumItem } from '../services/itensBazar.js';
import { api } from '../services/api.js';

const TAMANHO_MAXIMO_IMAGEM = 3 * 1024 * 1024;
const TIPOS_DE_IMAGEM_PERMITIDOS = ['image/jpeg', 'image/png', 'image/webp'];

export default function FormularioItemBazar({ item, onSalvar, onOcupado, onSemAutorizacao, disabled = false }) {
  const editando = Boolean(item);
  const [dados, setDados] = useState({
    nome: item?.nome ?? '', descricao: item?.descricao ?? '', preco: item?.preco ?? '',
    quantidadeDisponivel: item?.quantidadeDisponivel ?? '', categoria: item?.categoria ?? '',
    condicao: editando ? normalizarEnumItem(item.condicao, 'condicao') : 2,
    status: editando ? normalizarEnumItem(item.status, 'status') : 0,
  });
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);
  const emCurso = useRef(false);
  const [imagemSelecionada, setImagemSelecionada] = useState(null);
  const [previewImagem, setPreviewImagem] = useState(item?.imagemUrl ?? '');
  const [removerImagem, setRemoverImagem] = useState(false);

  function alterar(evento) {
    const { name, value } = evento.target;
    setDados((anterior) => ({ ...anterior, [name]: value }));
  }

  function selecionarImagem(evento) {
    const arquivo = evento.target.files?.[0];
    if (!arquivo) return;
    if (!TIPOS_DE_IMAGEM_PERMITIDOS.includes(arquivo.type)) {
      setErro('Formato não permitido. Use JPG, JPEG, PNG ou WEBP.');
      evento.target.value = '';
      return;
    }
    if (arquivo.size > TAMANHO_MAXIMO_IMAGEM) {
      setErro('A imagem deve ter no máximo 3 MB.');
      evento.target.value = '';
      return;
    }
    setErro('');
    setImagemSelecionada(arquivo);
    setPreviewImagem(URL.createObjectURL(arquivo));
    setRemoverImagem(false);
  }

  function removerImagemSelecionada() {
    setImagemSelecionada(null);
    setPreviewImagem('');
    setRemoverImagem(true);
  }

  async function enviar(evento) {
    evento.preventDefault();
    if (emCurso.current || disabled) return;
    const preco = Number(dados.preco);
    const quantidade = Number(dados.quantidadeDisponivel);
    if (!dados.nome.trim() || !dados.categoria.trim()) {
      setErro('Preencha o nome e a categoria.'); return;
    }
    if (dados.preco === '' || !Number.isFinite(preco) || preco < 0 ||
        dados.quantidadeDisponivel === '' || !Number.isInteger(quantidade) || quantidade < 0 || quantidade > 2147483647) {
      setErro('Informe um preço não negativo e uma quantidade inteira válida.'); return;
    }
    if (dados.condicao === '' || ![0, 1, 2].includes(Number(dados.condicao)) || (editando && (dados.status === '' || ![0, 1, 2].includes(Number(dados.status))))) {
      setErro('Selecione uma condição e um status válidos.'); return;
    }
    emCurso.current = true;
    setEnviando(true);
    onOcupado?.(true);
    setErro('');
    try {
      let imagemUrl = item?.imagemUrl ?? null;
      if (imagemSelecionada) {
        const dadosImagem = new FormData();
        dadosImagem.append('arquivo', imagemSelecionada);
        const respostaUpload = await api.post('/api/upload/imagem?pasta=produtos', dadosImagem);
        imagemUrl = respostaUpload?.url ?? respostaUpload?.Url;
        if (!imagemUrl) throw new Error('A API não retornou a URL da imagem.');
      } else if (removerImagem) {
        imagemUrl = null;
      }

      const corpo = {
        nome: dados.nome.trim(), descricao: dados.descricao.trim() || null, imagemUrl, preco,
        quantidadeDisponivel: quantidade, categoria: dados.categoria.trim(), condicao: Number(dados.condicao),
        ...(editando ? { status: Number(dados.status) } : {}),
      };
      await onSalvar(corpo);
    }
    catch (e) {
      if (e.status === 403 && onSemAutorizacao) onSemAutorizacao();
      else setErro(mensagemErroItem(e));
    }
    finally { emCurso.current = false; setEnviando(false); onOcupado?.(false); }
  }

  return (
    <form onSubmit={enviar} aria-busy={enviando}>
      <p>Campos com * são obrigatórios.</p>
      {erro && <p role="alert" className="formulario-bazar-erro">{erro}</p>}
      <fieldset disabled={enviando || disabled}>
        <legend>Dados do item</legend>
        <label htmlFor="formulario-bazar-imagem">Imagem do produto</label>
        {previewImagem && (
          <div className="formulario-bazar-imagem-previa">
            <img src={previewImagem} alt="Pré-visualização da imagem do produto" />
            <button type="button" onClick={removerImagemSelecionada}>Remover imagem</button>
          </div>
        )}
        <input id="formulario-bazar-imagem" name="imagem" type="file"
          accept="image/jpeg,image/png,image/webp" onChange={selecionarImagem} />
        <p className="formulario-bazar-ajuda">JPG, PNG ou WEBP, até 3 MB. Opcional.</p>
        <label htmlFor="formulario-bazar-nome">Nome *</label>
        <input id="formulario-bazar-nome" name="nome" value={dados.nome} onChange={alterar} required />
        <label htmlFor="formulario-bazar-descricao">Descrição</label>
        <textarea id="formulario-bazar-descricao" name="descricao" value={dados.descricao} onChange={alterar} rows={4} />
        <div className="formulario-bazar-colunas">
          <div>
            <label htmlFor="formulario-bazar-preco">Preço (R$) *</label>
            <input id="formulario-bazar-preco" name="preco" type="number" min="0" step="0.01" value={dados.preco} onChange={alterar} required />
          </div>
          <div>
            <label htmlFor="formulario-bazar-quantidade">Quantidade disponível *</label>
            <input id="formulario-bazar-quantidade" name="quantidadeDisponivel" type="number" min="0" max="2147483647" step="1" value={dados.quantidadeDisponivel} onChange={alterar} required />
          </div>
        </div>
        <label htmlFor="formulario-bazar-categoria">Categoria *</label>
        <input id="formulario-bazar-categoria" name="categoria" placeholder="Ex.: Livro, Calculadora" value={dados.categoria} onChange={alterar} required />
        <label htmlFor="formulario-bazar-condicao">Condição{editando ? '' : ' *'}</label>
        <select id="formulario-bazar-condicao" name="condicao" value={dados.condicao} onChange={alterar} required>
          <option value="" disabled>Selecione a condição</option>
          <option value="0">Novo</option><option value="1">Seminovo</option><option value="2">Usado</option>
        </select>
        {editando && <>
          <label htmlFor="formulario-bazar-status">Status</label>
          <select id="formulario-bazar-status" name="status" value={dados.status} onChange={alterar} required>
            <option value="" disabled>Selecione o status</option>
            <option value="0">Disponível</option><option value="1">Esgotado</option><option value="2">Inativo</option>
          </select>
        </>}
        {editando && <p className="formulario-bazar-ajuda">Condição e status já estão preenchidos. Altere apenas se necessário.</p>}
        <button className="formulario-bazar-primario" type="submit">{enviando ? 'Salvando…' : editando ? 'Salvar alterações' : 'Cadastrar item'}</button>
      </fieldset>
    </form>
  );
}
