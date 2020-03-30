<template>
  <div class="container">
    <div class="upper-section">
      <codemirror class="editor" v-model="code" :options="cmOptions"></codemirror>
      <div class="options">
        <input class="text-input" type="text" placeholder="Times to run" />
        <button @click="runCode" class="btn">Run!</button>
      </div>
      <div class="errors" v-if="errors && errors.length > 0">
        <div v-for="error in errors" :key="error" class="error">{{error}}</div>
      </div>
    </div>
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320">
      <path
        fill="#0099ff"
        fill-opacity="1"
        d="M0,32L120,32C240,32,480,32,720,42.7C960,53,1200,75,1320,85.3L1440,96L1440,0L1320,0C1200,0,960,0,720,0C480,0,240,0,120,0L0,0Z"
      />
    </svg>
  </div>
</template>

<script>
import "codemirror/lib/codemirror.css";
import "codemirror/theme/nord.css";
import { codemirror } from "vue-codemirror";
import "codemirror/mode/clike/clike";

export default {
  data() {
    return {
      code:
        "using System;\n\nnamespace CodeEnv{\n\tpublic class Test{\n\t\tpublic void Run(){\n\t\t//Write your code here\n\t\t}\n\t}\n}",
      cmOptions: {
        tabSize: 4,
        mode: "text/x-csharp",
        theme: "nord",
        lineNumbers: true,
        line: true
      },
      errors: []
    };
  },
  components: {
    codemirror
  },
  methods: {
    runCode() {
      let stripped = JSON.stringify(
        this.code.replace(/\/\*[\s\S]*?\*\/|\/\/.*/g, "")
      );
      stripped = stripped.replace(/\\n|\\t|\\r/gm, "");
      let api = this.$store.getters.api + "/Code";
      this.$http.post(api, { code: stripped }).then(res => {
        let data = res.data;
        if (data.errors) {
          for (let i = 0; i < data.errors.length; i++) {
            let closing = data.errors[i].indexOf(")") + 1
            let lineStart = data.errors[i].indexOf(',')+1
            let sub = data.errors[i].substring(lineStart, closing-1);
            data.errors[i] = this.getLine(sub) + data.errors[i].substring(closing)
          }
        }
        this.errors = data.errors;
        console.log(res);
      });
    },
    getLine(index) {
      let codeLines = this.code.replace(/\/\*[\s\S]*?\*\/|\/\/.*/g, "").split("\n");
      var tot = 0;
      for (let i = 0; i < codeLines.length; i++) {
        tot += codeLines[i].length;
        if (tot >= index) {
          return "(1, "+(i + 1) + ")";
        }
      }
    }
  },
  computed: {
    codemirror() {
      return this.$refs.cmEditor.codemirror;
    }
  }
};
</script>

<style scoped>
.container {
  text-align: left;
}

.upper-section {
  background-color: #0099ff;
  height: auto;
  padding: 50px 5px;
}

.editor {
  width: 70%;
  margin: 0 auto;
}

.options {
  width: 70%;
  margin: 10px auto;
  display: flex;
  justify-content: space-between;
}

.errors {
  background-color: #2e3440;
  width: 68%;
  margin: 0 auto;
  padding: 10px;
}
.error {
  color: #bf616a;
  margin: 3px;
}

.btn {
  background-color: #005d9b;
  width: 302px;
  border: 0;
  padding: 15px 0;
  margin: 5px 0;
  text-align: center;
  color: #fff;
  font-weight: bold;
}

.text-input {
  padding: 15px 10px;
}
</style>