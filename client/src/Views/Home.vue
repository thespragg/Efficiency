<template>
  <div class="container">
    <div class="upper-section">
      <div class="info">
        <img class="tip-img" src="@/assets/tip.png"/>
        <p>Tip: Use Output.WriteLine(string text) to print messages to the console!</p>
      </div>
      <codemirror class="editor" v-model="code" :options="cmOptions"></codemirror>
      <div class="console" v-if="hasLogToDisplay">
        <h4>Console Output</h4>
        <div class="break"></div>
        <div v-for="log in logs" :key="log" class="log">{{log}}</div>
        <div v-for="error in errors" :key="error" class="error">{{error}}</div>
      </div>
      <div class="options">
        <input class="text-input" type="text" placeholder="Times to run" />
        <button @click="runCode" class="btn">Run!</button>
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
        "using System;\n\nnamespace CodeEnv{\n\t//Don't change the class name\n\tpublic class SandBox{\n\t\tpublic void Run(){\n\t\t//Write your code here\n\t\t}\n\t}\n}",
      cmOptions: {
        tabSize: 4,
        mode: "text/x-csharp",
        theme: "nord",
        lineNumbers: true,
        line: true
      },
      errors: [],
      logs: []
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
            let closing = data.errors[i].indexOf(")") + 1;
            let lineStart = data.errors[i].indexOf(",") + 1;
            let sub = data.errors[i].substring(lineStart, closing - 1);
            data.errors[i] =
              this.getLine(sub) + data.errors[i].substring(closing);
          }
        }
        this.errors = data.errors;
        this.logs = data.consoleLogs;
        console.log(res);
      });
    },
    getLine(index) {
      let codeLines = this.code
        .replace(/\/\*[\s\S]*?\*\/|\/\/.*/g, "")
        .split("\n");
      var tot = 0;
      for (let i = 0; i < codeLines.length; i++) {
        tot += codeLines[i].length;
        if (tot >= index) {
          return "(1, " + (i + 1) + ")";
        }
      }
    }
  },
  computed: {
     hasLogToDisplay(){
      if(this.errors && this.errors.length > 0){
        return true;
      }
      if(this.logs && this.logs.length > 0){
        return true;
      }
      return false;
    }
  }
};
</script>

<style scoped>
.container {
  text-align: left;
}

.tip-img{
height:32px;
margin-left: 20px;
}

.upper-section {
  background-color: #0099ff;
  height: auto;
  padding: 50px 5px;
}

.info{
  background-color: #016FB9;
  color: white;
  border-radius: 16px;
  width: 70%;
  margin: 10px auto;
  display:flex;
  align-items: center;
}

.info p{
  padding:20px;
  margin:0;
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

.error {
  color: #bf616a;
  padding: 5px;
}

.console {
  background-color: #2e3440;
  width: 70%;
  margin: 10px auto;
  box-sizing: border-box;
  padding: 10px;
  font-size: 13px;
  font-family: "monospace", sans-serif;
  border-radius: 16px;
}

.log {
  color: #8FBCBB;
  padding:5px;
}

.console h4{
  padding: 5px;
  margin: 0;
  color: #E5E9F0;
  font-weight: 300;
}

.break{
  width: 100%;
  border-bottom: solid 1px #D8DEE9;
  margin-bottom: 10px;
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

.btn:hover{
  background-color: #033557;
}

.text-input {
  padding: 15px 10px;
}
</style>