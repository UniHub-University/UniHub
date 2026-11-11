import { useRef, useState } from 'react';
import { ApiError, api } from '../services/api.js';

const TAMANHO_MAXIMO_IMAGEM = 3 * 1024 * 1024;
const TIPOS_DE_IMAGEM_PERMITIDOS = ['image/jpeg', 'image/png', 'image/webp'];

function horarioParaInput(valor) {
  if (!valor || typeof valor !== 'string') return '';
  const partes = valor.split(':');
  if (partes.length < 2) return '';
  const [horas, minutos] = partes;
  return `${horas.padStart(2, '0')}:${minutos.padStart(2, '0')}`;
}

// Mensagens de erro padronizadas (mesmo comportamento do módulo Bazar)
function mensagemErroAlimento(erro) {
  if (erro instanceof ApiError) {
    if (erro.status === 401) return 'Sua sessão expirou. Entre novamente para continuar.';
    if (erro.status === 403) return 'Você não tem autorização para editar/excluir esse alimento.';
    if (erro.status === 404) return 'Este alimento não foi encontrado. Ele pode ter sido excluído.';
    if (erro.status >= 500) return 'O servidor não conseguiu concluir a operação. Tente novamente mais tarde.';
    return erro.message;
  }
  return 'Não foi possível concluir a operação. Tente novamente.';
}

export default function FormularioAlimento({ alimento, onSalvar, onOcupado, onSemAutorizacao, disabled = false }) {
  const editando = Boolean(alimento);
  const [dados, setDados] = useState({
    nome: alimento?.nome ?? '',
    descricao: alimento?.descricao ?? '',
    preco: alimento?.preco ?? '',
    quantidadeDisponivel: alimento?.quantidadeDisponivel ?? '',
    categoria: alimento?.categoria ?? '',
    restricoes: alimento?.restricoes && alimento.restricoes !== 'null' ? alimento.restricoes : '',
    horarioDisponibilidade: horarioParaInput(alimento?.horarioDisponibilidade),
  });
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);
  const emCurso = useRef(false);
  const [imagemSelecionada, setImagemSelecionada] = useState(null);
  const [previewImagem, setPreviewImagem] = useState(alimento?.imagemUrl ?? '');
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
      setErro('Preencha o nome e a categoria.');
      return;
    }
    if (dados.preco === '' || !Number.isFinite(preco) || preco < 0 ||
        dados.quantidadeDisponivel === '' || !Number.isInteger(quantidade) || quantidade < 0 || quantidade > 2147483647) {
      setErro('Informe um preço não negativo e uma quantidade inteira válida.');
      return;
    }
    if (!dados.horarioDisponibilidade) {
      setErro('Informe o horário de disponibilidade.');
      return;
    }

    emCurso.current = true;
    setEnviando(true);
    onOcupado?.(true);
    setErro('');
    try {
      let imagemUrl = alimento?.imagemUrl ?? null;
      if (imagemSelecionada) {
        const dadosImagem = new FormData();
        dadosImagem.append('arquivo', imagemSelecionada);
        const respostaUpload = await api.post('/api/upload/imagem?pasta=produtos', dadosImagem);
        imagemUrl = respostaUpload?.url ?? respostaUpload?.Url;
        if (!imagemUrl) throw new Error('A API não retornou a URL da imagem.');
      } else if (removerImagem) {
        imagemUrl = null;
      }

      // O backend espera HorarioDisponibilidade como TimeSpan ("HH:mm:ss").
      const corpo = {
        nome: dados.nome.trim(),
        descricao: dados.descricao.trim() || null,
        imagemUrl,
        preco,
        quantidadeDisponivel: quantidade,
        categoria: dados.categoria.trim(),
        restricoes: dados.restricoes.trim() || null,
        horarioDisponibilidade: `${dados.horarioDisponibilidade}:00`,
      };
      
      await onSalvar(corpo);
    } catch (e) {
      if (e.status === 403 && onSemAutorizacao) onSemAutorizacao();
      else setErro(mensagemErroAlimento(e));
    } finally {
      emCurso.current = false;
      setEnviando(false);
      onOcupado?.(false);
    }
  }

  return (
    <form onSubmit={enviar} aria-busy={enviando}>
      <p>Campos com * são obrigatórios.</p>
      {erro && <p role="alert" className="formulario-bazar-erro">{erro}</p>}
      <fieldset disabled={enviando || disabled}>
        <legend>Dados do alimento</legend>

        <label htmlFor="formulario-alimento-imagem">Imagem do alimento</label>
        {previewImagem && (
          <div className="formulario-bazar-imagem-previa">
            <img src={previewImagem} alt="Pré-visualização da imagem do alimento" />
            <button type="button" onClick={removerImagemSelecionada}>Remover imagem</button>
          </div>
        )}
        <input id="formulario-alimento-imagem" name="imagem" type="file"
          accept="image/jpeg,image/png,image/webp" onChange={selecionarImagem} />
        <p className="formulario-bazar-ajuda">JPG, PNG ou WEBP, até 3 MB. Opcional.</p>

        <label htmlFor="formulario-alimento-nome">Nome *</label>
        <input id="formulario-alimento-nome" name="nome" value={dados.nome} onChange={alterar} required />

        <label htmlFor="formulario-alimento-descricao">Descrição</label>
        <textarea id="formulario-alimento-descricao" name="descricao" value={dados.descricao} onChange={alterar} rows={4} />

        <div className="formulario-bazar-colunas">
          <div>
            <label htmlFor="formulario-alimento-preco">Preço (R$) *</label>
            <input id="formulario-alimento-preco" name="preco" type="number" min="0" step="0.01" value={dados.preco} onChange={alterar} required />
          </div>
          <div>
            <label htmlFor="formulario-alimento-quantidade">Quantidade disponível *</label>
            <input id="formulario-alimento-quantidade" name="quantidadeDisponivel" type="number" min="0" max="2147483647" step="1" value={dados.quantidadeDisponivel} onChange={alterar} required />
          </div>
        </div>

        <label htmlFor="formulario-alimento-categoria">Categoria *</label>
        <input id="formulario-alimento-categoria" name="categoria" placeholder="Ex.: Doce, Salgado, Bebida" value={dados.categoria} onChange={alterar} required />

        <label htmlFor="formulario-alimento-restricoes">Restrições alimentares</label>
        <input id="formulario-alimento-restricoes" name="restricoes" placeholder="Ex.: Contém glúten, lactose, amendoim" value={dados.restricoes} onChange={alterar} />

        <label htmlFor="formulario-alimento-horario">Horário de disponibilidade *</label>
        <input id="formulario-alimento-horario" name="horarioDisponibilidade" type="time" value={dados.horarioDisponibilidade} onChange={alterar} required />

        <button className="formulario-bazar-primario" type="submit">
          {enviando ? 'Salvando…' : editando ? 'Salvar alterações' : 'Cadastrar alimento'}
        </button>
      </fieldset>
    </form>
  );
}
